using Framework;

using System;

namespace Framework.Scalability
{
    public static class RemoteAgentBuilder
    {
        public static RemoteAgent FromConfiguration(RemoteAgentConfiguration configuration) => new RemoteAgent(configuration);
    }
}
