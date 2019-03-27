using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Regi.Services
{
    public interface INetworkingService
    {
        IPEndPoint[] GetListeningPorts();
    }

    public class NetworkingService : INetworkingService
    {
        private readonly IPGlobalProperties _ipGlobalProperties;

        public NetworkingService()
        {
            _ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        }

        public IPEndPoint[] GetListeningPorts()
        {
            return _ipGlobalProperties.GetActiveTcpListeners();
        }
    }
}
