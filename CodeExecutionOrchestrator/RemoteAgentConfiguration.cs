using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace CodeExecutionOrchestrator
{
    public class RemoteAgentConfiguration
    {
        public int Port { get; private set; }

        public RemoteAgentConfiguration(int port)
        {
            Port = port;
        }

        public RemoteAgentConfiguration()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            Port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
        }

        public string[] StartupArguments { get; private set; }
    }
}
