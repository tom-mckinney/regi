using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
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
        IRuntimeInfo RuntimeInfo { get; }
        Process GetKillProcess(string processName, RegiOptions options);
        void RunAnonymousScript(string script, RegiOptions options);
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

        public Process GetKillProcess(string processName, RegiOptions options)
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

            process.OutputDataReceived += (o, e) =>
            {
                if (options.Verbose && !string.IsNullOrWhiteSpace(e.Data))
                {
                    _console.WriteLine(e.Data);
                }
            };
            process.ErrorDataReceived += (o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    _console.WriteWarningLine(e.Data);
                }
            };

            return process;
        }

        public void RunAnonymousScript(string script, RegiOptions options)
        {
            string scriptExecutable = PathUtility.GetFileNameFromCommand(script);
            if (!PathUtility.TryGetPathFile(scriptExecutable, RuntimeInfo, out string fileName))
            {
                fileName = RuntimeInfo.IsWindows ? "powershell.exe" : "bash";
            }
            else
            {
                script = script.Remove(0, scriptExecutable.Length);
            }

            if (options.Verbose)
            {
                _console.WriteDefaultLine($"Executing '{fileName} {script}'");
            }

            using (var process = ProcessUtility.CreateProcess(fileName, script))
            {
                process.ErrorDataReceived += ProcessUtility.WriteOutput(_console, ConsoleLogLevel.Error);
                if (options.Verbose)
                {
                    process.OutputDataReceived += ProcessUtility.WriteOutput(_console);
                }

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit(5000);
            }
        }
    }
}
