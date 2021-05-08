using CodeExecutionOrchestrator.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeExecutionOrchestrator.Agent.Infrastructure
{
    public static class AgentConfiguration
    {
        public static AgentInfo Agent { get; set; } = new AgentInfo();

        public static string ServerAddress { get; set; }

        public static int Port { get; set; }
    }
}
