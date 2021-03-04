using Framework.Scalability.Agent;
using Framework.Scalability.Agent.Infrastructure;
using Framework.Scalability.Core;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace Framework
{
    public sealed class RemoteAgent : IDisposable
    {
        private readonly RemoteAgentConfiguration _configuration;

        private readonly ILogger _logger;

        private readonly Dictionary<string, (Delegate Action, Type ReturnType)> _actionDictionary;

        private string _transform(string source) => source.Trim().ToUpper();

        public RemoteAgent(RemoteAgentConfiguration configuration)
        {
            _configuration = configuration;
            _actionDictionary = new Dictionary<string, (Delegate Action, Type ReturnType)>();
        }

        public RemoteAgent(RemoteAgentConfiguration configuration, ILogger logger) : this(configuration)
        {
            _logger = logger;
        }

        //How do you optimize this???
        #region Actions 

        public void On(string eventName, Action action) => _actionDictionary.Add(_transform(eventName), (action, null));

        public void On<T>(string eventName, Action<T> action) => _actionDictionary.Add(_transform(eventName), (action, null));

        public void On<T1, T2>(string eventName, Action<T1, T2> action) => _actionDictionary.Add(_transform(eventName), (action, null));

        #endregion

        #region Funcs

        public void On<TRet>(string eventName, Func<TRet> action)=> _actionDictionary.Add(_transform(eventName), (action, typeof(TRet)));

        public void On<T1, TRet>(string eventName, Func<T1, TRet> action) => _actionDictionary.Add(_transform(eventName), (action, typeof(TRet)));

        public void On<T1, T2, TRet>(string eventName, Func<T1, T2, TRet> action) => _actionDictionary.Add(_transform(eventName), (action, typeof(TRet)));

        #endregion

        public void Dispose()
        {

        }

        public void Run()
        {
            Task.Run(() => Program.Main(_configuration.Port));
            APIControllerIOC.NINOInvoked += async(sender, e) =>
            {
                if (_actionDictionary.ContainsKey(_transform(e.ActionName)))
                {
                    try
                    {
                        await InvokeDelegateAsync(_actionDictionary[_transform(e.ActionName)].Action);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        if (_logger != null)
                        {
                            _logger.LogError(ex.GetType().ToString(), ex);
                        }
                    }
                }
            };

            APIControllerIOC.SINOInvoked += async(sender, e) =>
            {
                if (_actionDictionary.ContainsKey(_transform(e.ActionName)))
                {
                    try
                    {
                        await InvokeDelegateAsync(_actionDictionary[_transform(e.ActionName)].Action, e.Parameters);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        if (_logger != null)
                        {
                            _logger.LogError(ex.GetType().ToString(), ex);
                        }
                    }
                }
            };

            APIControllerIOC.NISOInvoked += async(sender, e) =>
            {
                if (_actionDictionary.ContainsKey(_transform(e.ActionName)))
                {
                    var result = default(object);
                    try
                    {
                        result = await InvokeDelegateAsync(_actionDictionary[_transform(e.ActionName)].Action);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        if (_logger != null)
                        {
                            _logger.LogError(ex.GetType().ToString(), ex);
                        }
                    }
                    finally
                    {
                        APIControllerIOC.OnNISOCallback(e, new Framework.Scalability.Core.InstanceDescriptor(_actionDictionary[_transform(e.ActionName)].ReturnType, result)); //Always return something, don't leave the server hanging
                    }
                }
            };

            APIControllerIOC.SISOInvoked += async(sender, e) =>
            {
                if (_actionDictionary.ContainsKey(_transform(e.ActionName)))
                {
                    var result = default(object);
                    try
                    {
                        result = await InvokeDelegateAsync(_actionDictionary[_transform(e.ActionName)].Action, e.Parameters);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        if (_logger != null)
                        {
                            _logger.LogError(ex.GetType().ToString(), ex);
                        }
                    }
                    finally
                    {
                        APIControllerIOC.OnSISOCallback(e, new Framework.Scalability.Core.InstanceDescriptor(_actionDictionary[_transform(e.ActionName)].ReturnType, result)); //Always return something, don't leave the server hanging
                    }
                }
            };
        }


        //https://stackoverflow.com/questions/51012710/when-to-await-a-dynamically-invoked-method
        private static async Task<object> InvokeDelegateAsync(Delegate method) => await InvokeDelegateAsync(method, (object[])null);
        private static async Task<object> InvokeDelegateAsync(Delegate method, InstanceDescriptor[] parameters) => await InvokeDelegateAsync(method, parameters.Select(x => Convert.ChangeType(x.Instance, x.Type)).ToArray());
        private static async Task<object> InvokeDelegateAsync(Delegate method, object[] parameters)
        {
            var result = default(object);
            if (method.Method.ReturnType.IsSubclassOf(typeof(Task)))
            {
                if (method.Method.ReturnType.IsConstructedGenericType)
                {
                    dynamic tmp = method.DynamicInvoke(parameters);
                    result = await tmp;
                }
                else
                {
                    await (method.DynamicInvoke(parameters) as Task);
                }
            }
            else
            {
                result = method.DynamicInvoke(parameters);
            }
            return result;
        }

        public void Terminate()
        {

        }
    }
}
