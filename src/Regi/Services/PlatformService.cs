using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Services
{
    public interface IPlatformService
    {
        Process GetKillProcess(string processName);
        IRuntimeInfo RuntimeInfo { get; }
    }

    public class PlatformService : IPlatformService
    {
        private readonly IConsole _console;

        public PlatformService(IConsole console, IRuntimeInfo runtimeInfo)
        {
            _console = console;
            RuntimeInfo = runtimeInfo;
        }

        public IRuntimeInfo RuntimeInfo { get; private set; }

        public Process GetKillProcess(string processName)
        {
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            if (RuntimeInfo.IsWindows)
            {
                startInfo.FileName = "taskkill";
                startInfo.Arguments = $"/F /IM {ProcessUtility.AddExtension(processName)}";
            }
            else
            {
                startInfo.FileName = "killall";
                startInfo.Arguments = processName;
            }

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true,
            };

            process.OutputDataReceived += (o, e) => _console.WriteLine(e.Data);
            process.ErrorDataReceived += (o, e) => _console.WriteErrorLine(e.Data);

            return process;
        }
    }
}
