using CodeExecutionOrchestrator;

using CodeExecutionOrchestrator;

using Newtonsoft.Json;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CodeExecutionOrchestrator.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ShowWindow(ThisConsole, MAXIMIZE);
            var random = new Random();
            var agent = RemoteAgentBuilder.FromConfiguration(new RemoteAgentConfiguration());
            agent.On("NINOTest", () =>
            {
                Console.WriteLine("yes");
            });

            agent.On("TestWithResult", () =>
            {
                Console.WriteLine("Test with result");
                return 0;
            });

            agent.On("NISOTest", () =>
            {
                Console.WriteLine("NISO test");
                return 42;
            });

            agent.On("LongNISOTest", async() =>
            {
                Console.WriteLine("Doing work...");
                await Task.Delay(random.Next(5000, 10000));
                Console.WriteLine("Work completed");
                return 544;
            });

            agent.On("SINOTest", (int x) =>
            {
                Console.WriteLine("SINO: " + x);
            });

            agent.On("SINOMultiParameterTest", (int x, double y) =>
            {
                Console.WriteLine($"SINO Multi parameter test: x:{x} y:{y}");
            });

            agent.On("SISOTest", (int x) =>
            {
                Console.WriteLine("SISO: " + x);
                return x + 1;
            });

            agent.On("SISOMultiParameterTest", (int x, double y) =>
            {
                Console.WriteLine($"SISO Multi parameter test: x:{x} y:{y}");
                return x + y;
            });

            agent.Run();
            Console.Clear();
            Console.CancelKeyPress += (_, __) =>
            {
                agent.Terminate();
            };
            Console.WriteLine("Some project started");
            while (!Console.KeyAvailable)
                await Task.Delay(100);
        }



        [DllImport("kernel32.dll", ExactSpelling = true)]

        private static extern IntPtr GetConsoleWindow();
        private static IntPtr ThisConsole = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;
    }

}
