using CodeExecutionOrchestrator.Core;
using CodeExecutionOrchestrator.Core.Actions;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeExecutionOrchestrator.Agent.Infrastructure
{
    public static class ActionCallbackHandler
    {

        private static Random _random = new Random();

        private static readonly Dictionary<RemoteActionInvocationArgs, int> _callbackIDDictionary = new Dictionary<RemoteActionInvocationArgs, int>();

        private static HttpHelper _helper;

        private static string _serverURL;
        public static string ServerURL
        {
            get
            {
                return _serverURL;
            }
            set
            {
                _helper = new HttpHelper(value);
                _serverURL = value;
            }
        }

        static ActionCallbackHandler()
        {
            //Handle callbacks
            APIControllerIOC.NISOCallback += APIControllerIOC_SOCallback;
            APIControllerIOC.SISOCallback += APIControllerIOC_SOCallback;
        }

        private static async void APIControllerIOC_SOCallback(object sender, InstanceDescriptor e)
        {
            if (sender is RemoteActionInvocationArgs)
            {
                var action = sender as RemoteActionInvocationArgs;
                var callbackID = _callbackIDDictionary[action];
                if (_helper != null)
                {
                    await _helper.POSTRequestAsync<string>("InvokeCallback?id=" + callbackID, JsonConvert.SerializeObject(e.Instance, e.Type, new JsonSerializerSettings()));
                }
                _callbackIDDictionary.Remove(action);
            }
        }

        public static int Register(RemoteActionInvocationArgs args)
        {
            _callbackIDDictionary.Add(args, _random.Next(int.MaxValue));
            return _callbackIDDictionary[args];
        }
    }
}
