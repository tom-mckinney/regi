using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Services
{
    public interface INetworkingService
    {
        IPEndPoint[] GetListeningPorts();
    }

    public class NetworkingService : INetworkingService
    {
        private readonly IConsole _console;
        private readonly IPGlobalProperties _ipGlobalProperties;
        private static readonly bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public NetworkingService(IConsole console)
        {
            _console = console;
            _ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        }

        public IPEndPoint[] GetListeningPorts()
        {
            try
            {
                return _ipGlobalProperties.GetActiveTcpListeners();
            }
            catch
            {
                _console.WriteErrorLine("Could not get active Tcp listeners. Are you running in Windows Subsystem for Linux? This is currently not supported.");

                throw;

                //if (_isLinux)
                //{
                //    Console.WriteLine("Very wumbo");
                //    Process netstat = new Process
                //    {
                //        StartInfo = new ProcessStartInfo
                //        {
                //            FileName = "netstat",
                //            Arguments = $"-tna",
                //            RedirectStandardOutput = true
                //        }
                //    };

                //    netstat.StandardOutput.ReadToEnd();

                //    Process grep1 = new Process
                //    {
                //        StartInfo = new ProcessStartInfo
                //        {
                //            FileName = "grep",
                //            Arguments = $"LISTEN",
                //            RedirectStandardOutput = true,
                //            RedirectStandardInput = true,
                            
                //        }
                //    };

                    
                //    netstat.Start();
                //    netstat.WaitForExit();

                //    grep1.Start();
                //    grep1.StandardInput.WriteAsync(netstat.StandardOutput.ReadToEnd());

                //    Console.WriteLine(grep1.StandardOutput.ReadToEnd());
                //    grep1.WaitForExit();

                //    return new IPEndPoint[] { };
                //}
                //else
                //{
                //    throw;
                //}
            }
        }
    }
}
