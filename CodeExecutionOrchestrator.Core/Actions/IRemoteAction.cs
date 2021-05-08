using System;
using System.Collections.Generic;
using System.Text;

namespace CodeExecutionOrchestrator.Core.Actions
{
    public interface IRemoteAction<T>
    {
        public string Name { get; }
    }
}
