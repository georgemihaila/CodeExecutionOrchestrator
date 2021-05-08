using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodeExecutionOrchestrator.Core.Actions
{
    public class RemoteActionInvocationArgs : EventArgs
    {
        public string ActionName { get; set; }

        public InstanceDescriptor[] Parameters { get; set; }

        public RemoteActionInvocationArgs(string actionName)
        {
            ActionName = actionName;
        }
        public RemoteActionInvocationArgs(string actionName, InstanceDescriptor[] parameters) : this(actionName)
        {
            Parameters = parameters;
        }
    }
}
