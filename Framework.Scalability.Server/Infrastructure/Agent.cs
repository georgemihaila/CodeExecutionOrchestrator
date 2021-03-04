using Framework.Scalability.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Scalability.Server.Infrastructure
{
    public class Agent : AgentInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AgentStatus Status { get; set; }

        public DateTime LastUpdate { get; set; }

        public int UnreachabilityCount { get; private set; } = 0;

        public async Task UpdateStatusAsync()
        {
            var httpHelper = new HttpHelper(Address);
            try
            {
                if (await httpHelper.GETRequestAsync("/api/up") == "client up")
                {
                    Status = AgentStatus.Reachable;
                    UnreachabilityCount = 0;
                }
            }
            catch
            {
                if (++UnreachabilityCount <= 3)
                {
                    Console.WriteLine($"Agent @{Address} unreachable ({UnreachabilityCount}/{3})");
                }
                Status = AgentStatus.Unreachable;
            }
            finally
            {
                LastUpdate = DateTime.Now;
            }
        }
    }
     
    public enum AgentStatus { Reachable, Unreachable }
}
