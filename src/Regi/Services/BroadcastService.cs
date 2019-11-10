using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IBroadcastService
    {
        IDictionary<string, NamedPipeServerStream> ServerStreams { get; }

        Task StartBroadcastServer(CancellationToken cancellationToken);
        Task OpenStream();
        Task Subscribe(IList<Project> projects, CancellationToken cancellationToken);
    }

    public class BroadcastService : IBroadcastService
    {
        private CancellationToken _cancellationToken;

        public IDictionary<string, NamedPipeServerStream> ServerStreams { get; } = new Dictionary<string, NamedPipeServerStream>();

        public Task StartBroadcastServer(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            var serverThread = new Thread(async () =>
            {
                var protoServer = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls("https://localhost:5051/");
                        webBuilder.ConfigureServices(services =>
                        {
                            services.AddSingleton<IBroadcastService>(p => this);
                        });
                        webBuilder.UseStartup<ProtoServerStartup>();
                    })
                    .Build();

                await protoServer.RunAsync(_cancellationToken);
            });

            serverThread.Start();

            return Task.CompletedTask;
        }

        public async Task OpenStream()
        {
            var serverStream = new NamedPipeServerStream("regi_Backend", PipeDirection.Out);

            ServerStreams.Add("regi_Backend", serverStream);

            Console.Write("Waiting for client connection...");
            await serverStream.WaitForConnectionAsync(_cancellationToken);
            Console.WriteLine("Client connected.");

            using StreamWriter streamWriter = new StreamWriter(serverStream)
            {
                AutoFlush = true
            };

            await streamWriter.WriteLineAsync("Hellow world!");
        }

        public async Task Subscribe(IList<Project> projects, CancellationToken cancellationToken)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5051/");
            var client = new Handshake.HandshakeClient(channel);
            var reply = await client.ShakeHandsAsync(new HandshakeRequest { Apps = string.Join(',', projects.Select(p => p.Name)) }, cancellationToken: cancellationToken);

            using var clientStream = new NamedPipeClientStream(".", reply.Pipename, PipeDirection.In);

            Console.WriteLine("Connecting to server...\n");
            await clientStream.ConnectAsync(cancellationToken);

            Console.WriteLine("Connected to pipe.");
            Console.WriteLine("There are currently {0} pipe server instances open.", clientStream.NumberOfServerInstances);

            using StreamReader sr = new StreamReader(clientStream);

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
    }
}
