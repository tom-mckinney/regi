using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Regi.Commands
{
    [Command("broadcast", AllowArgumentSeparator = true)]
    public class BroadcastCommand : CommandBase
    {

        public BroadcastCommand(IBroadcastService broadcastService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console) : base(projectManager, configurationService, console)
        {
            _broadcastService = broadcastService;
        }

        private static NamedPipeServerStream PipeServer { get; set;  }

        private static StreamWriter _streamWriter;
        //private static StreamWriter StreamWriter
        //{
        //    get
        //    {
        //        if (_streamWriter == null)
        //        {
        //            _streamWriter = new StreamWriter(PipeServer);

        //            _streamWriter.AutoFlush = true;
        //        }

        //        return _streamWriter;
        //    }
        //}

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Apps.Concat(c.Tests);

        private readonly IBroadcastService _broadcastService;

        protected override int Execute(IList<Project> projects)
        {
            _broadcastService.ListenForBroadcastRequests();

            while (true)
            {
                console.WriteDefaultLine("Just chilling");

                Thread.Sleep(2000);
            }
        }

        protected int OldExecute(IList<Project> projects)
        {
            //TcpClient tcpClient = new TcpClient();

            //tcpClient.Connect("localhost", 11420);

            //tcpClient.GetStream().

            

            console.WriteWarningLine("THESE ARE NOT THE DROID YOU ARE LOOKING FOR!", ConsoleLineStyle.LineAfter);
            console.WriteWarningLine("This command is only to be used for inital testing of the hijack command.");

            Thread pipeServerThread = new Thread(TestThread);
            pipeServerThread.Start();

            for (int i = 0; true; i++)
            {
                var data = $"Rount {i}!";
                //if (PipeServer.IsConnected)
                //{
                //    StreamWriter.WriteLine();
                //}

                TryWriteToPipe(data);

                Thread.Sleep(1000);
            }
            //using (var PipeServer = new NamedPipeServerStream("regi_Backend", PipeDirection.InOut))
            //{
            //    Console.Write("Waiting for client connection...");
            //    //pipeServer.WaitForConnection();
            //    Console.WriteLine("Client connected.");

            //    //using (StreamWriter sw = new StreamWriter(pipeServer))
            //    {
            //        //sw.AutoFlush = true;


            //        //Console.Write("Enter text: ");

            //        //sw.WriteLine(Console.ReadLine());
            //    }
            //}

            return 0;
        }

        private static void TryWriteToPipe(string data)
        {
            if (_streamWriter != null)
            {
                _streamWriter.WriteLine(data);
            }
        }

        private static void TestThread(object data)
        {
            PipeServer = new NamedPipeServerStream("regi_Backend", PipeDirection.Out);
            {
                Console.Write("Waiting for client connection...");
                PipeServer.WaitForConnection();
                Console.WriteLine("Client connected.");

                _streamWriter = new StreamWriter(PipeServer)
                {
                    AutoFlush = true
                };

                //for (int i = 0; true; i++)
                //{
                //    if (PipeServer.IsConnected)
                //    {
                //        StreamWriter.WriteLine($"Rount {i}!");
                //    }

                //    Thread.Sleep(1000);
                //}
                //using (StreamWriter sw = new StreamWriter(pipeServer))
                {
                    //sw.AutoFlush = true;


                    //Console.Write("Enter text: ");

                    //sw.WriteLine(Console.ReadLine());
                }
            }
        }
    }
}
