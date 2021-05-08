using System;
using System.Collections.Generic;
using System.Text;

namespace CodeExecutionOrchestrator.Core.Actions
{
    public class TestAction : IRemoteAction<int>
    {
        public string Name => "test";
    }
}
