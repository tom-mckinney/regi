using System;
using System.Collections.Generic;
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
            //var reply = await client.SayHelloAsync(new HelloRequest { Name = "Wumbo!" });

            _console.WriteLine($"Greeting: {reply.Pipename}");

            return 0;
        }
    }
}
