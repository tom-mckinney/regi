using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        private object _lock = new object();

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
                StartInfo =
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
    }
}
