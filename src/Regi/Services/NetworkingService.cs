using McMaster.Extensions.CommandLineUtils;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Regi.Services
{
    public interface INetworkingService
    {
        bool IsPortListening(int port);

        string GetTcpMessage(string address, int port);
    }

    public class NetworkingService : INetworkingService
    {
        private readonly IConsole _console;
        private readonly IRuntimeInfo _runtime;
        private readonly IPGlobalProperties _ipGlobalProperties;

        public NetworkingService(IConsole console, IRuntimeInfo runtime)
        {
            _console = console;
            _runtime = runtime;
            
            if (_runtime.IsWindows)
            {
                _ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            }
        }

        public bool IsPortListening(int port)
        {
            if (_runtime.IsWindows && _ipGlobalProperties != null)
            {
                var activeTcpPorts = _ipGlobalProperties.GetActiveTcpListeners();

                return activeTcpPorts.Any(p => p.Port == port);
            }

            Process netstat = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName =  _runtime.IsWindows || _runtime.IsWindowsLinux ? "netstat.exe" : "netstat",
                    Arguments = "-tna",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                }
            };

            var output = string.Empty;

            netstat.OutputDataReceived += (o, e) =>
            {
                output += e.Data;
            };

            netstat.Start();
            netstat.BeginOutputReadLine();

            netstat.WaitForExit(2000);

            return ContainsNetstatPort(output, port);
        }

        public bool ContainsNetstatPort(string netstatResponse, int port)
        {
            return netstatResponse
                .Split(_runtime.NewLine)
                .Any(o =>
                {
                    bool isListening = o.Contains("LISTEN");

                    if (_runtime.IsMac)
                    {
                        isListening = isListening && Regex.IsMatch(o, $"[.:]{port}");
                    }
                    else
                    {
                        isListening = isListening && o.Contains($":{port}");
                    }

                    return isListening;
                });
        }

        public string GetTcpMessage(string address, int port)
        {
            string message = null;

            IPAddress localAddr = IPAddress.Parse(Constants.LocalAddress);
            TcpListener server = new TcpListener(localAddr, Constants.LocalPort);

            server.Start();

            byte[] bytes = new byte[256];

            while (message == null)
            {
                using (var client = server.AcceptTcpClient())
                {
                    using (var stream = client.GetStream())
                    {
                        int i;
                        while (stream.CanRead && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            message = Encoding.UTF8.GetString(bytes, 0, i);
                        }
                    }

                    client.Close();
                }
            }

            return message;
        }
    }
}
