using Microsoft.Extensions.Options;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Regi.Services
{
    public interface IBroadcastService
    {
        void ListenForBroadcastRequests();

        void RequestBroadcast(Project project);
    }

    public class BroadcastService : IBroadcastService
    {
        private static Thread _listenerThread;
        private readonly Settings _options;

        public BroadcastService(IOptions<Settings> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public void ListenForBroadcastRequests()
        {
            _listenerThread = new Thread(StartListener);

            _listenerThread.Start();
        }

        public void RequestBroadcast(Project project)
        {
            using (var client = new TcpClient(Constants.LocalAddress, Constants.LocalPort))
            {
                byte[] msg = Encoding.UTF8.GetBytes(project.Name);

                using (var stream = client.GetStream())
                {
                    stream.Write(msg, 0, msg.Length);
                }

                client.Close();
            }
        }

        private void StartListener()
        {
            IPAddress localAddr = IPAddress.Parse(Constants.LocalAddress);
            TcpListener server = new TcpListener(localAddr, Constants.LocalPort);

            server.Start();

            byte[] bytes = new byte[256];

            while (_options.RunIndefinitely)
            {
                using (var client = server.AcceptTcpClient())
                {
                    using (var stream = client.GetStream())
                    {
                        int i;
                        while (stream.CanRead && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            string data = Encoding.UTF8.GetString(bytes, 0, i);

                            Console.WriteLine(data); // Handle to create stream connection
                        }
                    }

                    client.Close();
                }
            }
        }
    }
}
