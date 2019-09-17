using McMaster.Extensions.CommandLineUtils;
using Regi.Utilities;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Regi.Services
{
    public interface INetworkingService
    {
        bool IsPortListening(int port);
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
            _ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        }

        public bool IsPortListening(int port)
        {
            if (_runtime.IsWindowsLinux || _ipGlobalProperties == null) // IPGlobalProperties is not implemented for WSL at this time
            {
                string netstat = _runtime.IsWindows || _runtime.IsWindowsLinux ? "netstat.exe" : "netstat";

                ProcessUtility.RunProcessAndWaitForExit(netstat, "-tna", out string output, out string _, 5_000);

                return ContainsNetstatPort(output, port);
            }

            var activeTcpPorts = _ipGlobalProperties.GetActiveTcpListeners();

            return activeTcpPorts.Any(p => p.Port == port);
        }

        public bool ContainsNetstatPort(string netstatResponse, int port)
        {
            return netstatResponse
                .Split(_runtime.NewLine)
                .Any(o =>
                {
                    bool isListening = o.Contains("LISTEN", StringComparison.InvariantCultureIgnoreCase);

                    if (_runtime.IsMac)
                    {
                        isListening = isListening && Regex.IsMatch(o, $"[.:]{port}");
                    }
                    else
                    {
                        isListening = isListening && o.Contains($":{port}", StringComparison.InvariantCultureIgnoreCase);
                    }

                    return isListening;
                });
        }
    }
}
