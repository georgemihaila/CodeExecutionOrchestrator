using CodeExecutionOrchestrator.Core;
using CodeExecutionOrchestrator.Core.Actions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeExecutionOrchestrator.Agent.Infrastructure
{
    public static class APIControllerIOC
    {
        #region No input no output
        
        public static event EventHandler<RemoteActionInvocationArgs> NINOInvoked;
        public static void OnNINOInvoked(object sender, RemoteActionInvocationArgs e) => NINOInvoked?.Invoke(sender, e);

        #endregion

        #region Some input no output

        public static event EventHandler<RemoteActionInvocationArgs> SINOInvoked;
        public static void OnSINOInvoked(object sender, RemoteActionInvocationArgs e) => SINOInvoked?.Invoke(sender, e);

        #endregion

        #region Some input some output

        public static event EventHandler<RemoteActionInvocationArgs> SISOInvoked;
        public static event EventHandler<InstanceDescriptor> SISOCallback;
        public static void OnSISOInvoked(object sender, RemoteActionInvocationArgs e) => SISOInvoked?.Invoke(sender, e);
        public static void OnSISOCallback(object sender, InstanceDescriptor e) => SISOCallback?.Invoke(sender, e);

        #endregion

        #region No input some output

        public static event EventHandler<RemoteActionInvocationArgs> NISOInvoked;

        public static event EventHandler<InstanceDescriptor> NISOCallback;

        public static void OnNISOInvoked(object sender, RemoteActionInvocationArgs e) => NISOInvoked?.Invoke(sender, e);
        public static void OnNISOCallback(object sender, InstanceDescriptor e) => NISOCallback?.Invoke(sender, e);

        #endregion
    }
}
