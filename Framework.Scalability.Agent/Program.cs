using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Framework.Scalability.Agent.Infrastructure;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Framework.Scalability.Agent
{
    public class Program
    {
        public static void Main(int port)
        {
            AgentConfiguration.Port = port;
            CreateHostBuilder(new string[] { "http://*:" + port }).Build().Run();
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (args != null)
                    {
                        webBuilder.UseUrls(args);
                    }
                    webBuilder.UseStartup<Startup>();
                });

    }
}
