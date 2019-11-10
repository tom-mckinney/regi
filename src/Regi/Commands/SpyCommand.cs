using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;

namespace Regi.Commands
{
    public class SpyCommand : CommandBase
    {
        public SpyCommand(IProjectManager projectManager, IConfigurationService configurationService, IConsole console) : base(projectManager, configurationService, console)
        {
        }

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Apps.Concat(c.Tests);

        protected override async Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5051/");
            var client = new Handshake.HandshakeClient(channel);
            var reply = await client.ShakeHandsAsync(new HandshakeRequest { Apps = string.Join(',', projects.Select(p => p.Name)) });

            _console.WriteLine($"Handshake: {reply.Pipename}");

            using (var PipeClient = new NamedPipeClientStream(".", $"regi_Backend", PipeDirection.In))
            {
                Console.WriteLine("Connecting to server...\n");
                await PipeClient.ConnectAsync(cancellationToken);

                Console.WriteLine("Connected to pipe.");
                Console.WriteLine("There are currently {0} pipe server instances open.", PipeClient.NumberOfServerInstances);

                using StreamReader sr = new StreamReader(PipeClient);

                while (!cancellationToken.IsCancellationRequested)
                {
                    string temp;
                    while ((temp = await sr.ReadLineAsync()) != null)
                    {
                        Console.WriteLine("Received from server: {0}", temp);
                    }
                    await Task.Delay(200);
                }
            }

            return 0;
        }
    }
}
