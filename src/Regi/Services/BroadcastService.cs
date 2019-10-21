using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IBroadcastService
    {
        Task StartBroadcastServer(CancellationToken cancellationToken);
    }

    public class BroadcastService : IBroadcastService
    {
        public IDictionary<string, NamedPipeServerStream> ServerStreams { get; } = new Dictionary<string, NamedPipeServerStream>();
        public IDictionary<string, NamedPipeClientStream> ClientStreams { get; } = new Dictionary<string, NamedPipeClientStream>();

        public Task StartBroadcastServer(CancellationToken cancellationToken)
        {
            var serverThread = new Thread(async () =>
            {
                var protoServer = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls("https://localhost:5051/");
                        webBuilder.UseStartup<ProtoServerStartup>();
                    })
                    .Build();

                await protoServer.RunAsync(cancellationToken);
            });

            serverThread.Start();

            return Task.CompletedTask;
        }

        public Task OpenServerStream()
        {
            ServerStreams.Add("")
            var PipeServer = new NamedPipeServerStream("regi_Backend", PipeDirection.Out);
        }
    }
}
