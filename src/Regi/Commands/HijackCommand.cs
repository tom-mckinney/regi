using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Regi.Commands
{
    [Command("hijack", AllowArgumentSeparator = true)]
    public class HijackCommand : CommandBase
    {
        private static int numClients = 4;
        private readonly IBroadcastService _broadcastService;

        public HijackCommand(IBroadcastService broadcastService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console) : base(projectManager, configurationService, console)
        {
            _broadcastService = broadcastService;
        }

        private static NamedPipeClientStream PipeClient { get; set; }

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Apps.Concat(c.Tests);

        protected override int Execute(IList<Project> projects)
        {
            foreach (var project in projects)
            {
                _broadcastService.RequestBroadcast(project);
            }

            return 0;
        }

        protected int OldExecute(IList<Project> projects)
        {
            console.WriteLine($"Hijacking {projects.Count} projects");

            //foreach (var project in projects)
            {
                using (PipeClient = new NamedPipeClientStream(".", $"regi_Backend", PipeDirection.In))
                {
                    Console.WriteLine("Connecting to server...\n");
                    PipeClient.Connect();

                    Console.WriteLine("Connected to pipe.");
                    Console.WriteLine("There are currently {0} pipe server instances open.",
                       PipeClient.NumberOfServerInstances);

                    using (StreamReader sr = new StreamReader(PipeClient))
                    {
                        while (true)
                        {
                            string temp;
                            while ((temp = sr.ReadLine()) != null)
                            {
                                Console.WriteLine("Received from server: {0}", temp);
                            }
                            Thread.Sleep(200);
                        }
                        // Display the read text to the console
                    }
                }
            }

            Console.Write("Press Enter to continue...");
            Console.ReadLine();

            return 0;
        }

        // Helper function to create pipe client processes
        private static void StartClients()
        {
            int i;
            string currentProcessName = Environment.CommandLine;
            Process[] plist = new Process[numClients];

            Console.WriteLine("Spawning client processes...\n");

            if (currentProcessName.Contains(Environment.CurrentDirectory))
            {
                currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, String.Empty);
            }

            // Remove extra characters when launched from Visual Studio
            currentProcessName = currentProcessName.Replace("\\", String.Empty);
            currentProcessName = currentProcessName.Replace("\"", String.Empty);

            for (i = 0; i < numClients; i++)
            {
                // Start 'this' program but spawn a named pipe client.
                plist[i] = Process.Start(currentProcessName, "spawnclient");
            }
            while (i > 0)
            {
                for (int j = 0; j < numClients; j++)
                {
                    if (plist[j] != null)
                    {
                        if (plist[j].HasExited)
                        {
                            Console.WriteLine("Client process[{0}] has exited.",
                                plist[j].Id);
                            plist[j] = null;
                            i--;    // decrement the process watch count
                        }
                        else
                        {
                            Thread.Sleep(250);
                        }
                    }
                }
            }
            Console.WriteLine("\nClient processes finished, exiting.");
        }
    }
}
