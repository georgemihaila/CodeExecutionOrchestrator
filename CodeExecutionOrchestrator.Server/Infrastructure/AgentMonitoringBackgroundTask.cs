using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeExecutionOrchestrator.Server.Infrastructure
{
    public class AgentMonitoringBackgroundTask : BackgroundService
    {
        private readonly List<Agent> _agents;
        private Timer _timer;

        public AgentMonitoringBackgroundTask(List<Agent> agents)
        {
            _agents = agents;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            _timer = new Timer(callback: async o => await DoWorkAsync(),
            state: null, dueTime: TimeSpan.FromSeconds(0),
            period: TimeSpan.FromSeconds(3));
            return Task.CompletedTask;
        }

        private async Task DoWorkAsync()
        {
            await Task.WhenAll(_agents.Select(x => Task.Run(async () => await x.UpdateStatusAsync())));
            foreach (var unreachable in _agents.Where(x => x.UnreachabilityCount >= 3).ToList())
            {
                _agents.Remove(unreachable);
                Console.WriteLine($"Removed agent @{unreachable.Address}");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            return Task.CompletedTask;
        }
    }
}
