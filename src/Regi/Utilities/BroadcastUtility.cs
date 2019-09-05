using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace Regi.Utilities
{
    public class BroadcastUtility : IDisposable
    {
        public static bool IsWaitingForConnection { get; private set; }

        public static bool HasClientConnections { get; private set; }

        public static Thread BroadcastThread { get; private set; }

        public static NamedPipeServerStream PipeServer { get; private set; }

        private static StreamWriter _streamWriter;

        public void StartBroadcast(Project project)
        {
            BroadcastThread = new Thread(InitializePipeServer(project));
        }

        private static ParameterizedThreadStart InitializePipeServer(Project project) => (object data) =>
        {
            PipeServer = new NamedPipeServerStream($"regi_{project.Name}");

            PipeServer.WaitForConnection();

            HasClientConnections = true;

            _streamWriter = new StreamWriter(PipeServer);
        };

        public static void PushMessage(string message)
        {
            if (HasClientConnections)
            {
                try
                {
                    _streamWriter.WriteLine(message);
                }
                catch (IOException)
                {
                    HasClientConnections = false;

                    PipeServer.Disconnect();
                }
            }
        }

        public void Dispose()
        {
            PipeServer.Dispose();
        }
    }
}
