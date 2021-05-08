using CodeExecutionOrchestrator;

using System;

namespace CodeExecutionOrchestrator
{
    public static class RemoteAgentBuilder
    {
        public static RemoteAgent FromConfiguration(RemoteAgentConfiguration configuration) => new RemoteAgent(configuration);
    }
}
