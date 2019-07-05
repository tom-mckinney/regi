using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Services
{
    public interface ICleanupService
    {
        void KillProcessTree(AppProcess process, RegiOptions options);
        void KillProcessTree(AppProcess process, RegiOptions options, TimeSpan timeout);
        IReadOnlyList<AppProcess> ShutdownBuildServers(RegiOptions options);
    }

    public class CleanupService : ICleanupService
    {
        private readonly IDotnetService dotnetService;
        private readonly IConsole console;
        private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);

        public CleanupService(IDotnetService dotnetService, IConsole console)
        {
            this.dotnetService = dotnetService;
            this.console = console;
        }

        public void KillProcessTree(AppProcess process, RegiOptions options)
        {
            KillProcessTree(process, options, _defaultTimeout);
        }

        public void KillProcessTree(AppProcess process, RegiOptions options, TimeSpan timeout)
        {
            string stdout;
            string stderr;

            if (_isWindows)
            {
                ProcessUtility.RunProcessAndWaitForExit("taskkill", $"/T /F /PID {process.ProcessId}", out stdout, out stderr);
                LogOutputs(stdout, stderr, options);
            }
            else
            {
                var children = new HashSet<int>();
                ProcessUtility.GetAllChildIdsUnix(process.ProcessId, children);
                foreach (var childId in children)
                {
                    ProcessUtility.KillProcessUnix(childId, out stdout, out stderr);
                    LogOutputs(stdout, stderr, options);
                }

                ProcessUtility.KillProcessUnix(process.ProcessId, out stdout, out stderr);
                LogOutputs(stdout, stderr, options);
            }

            process.Kill(timeout, console);
        }

        public IReadOnlyList<AppProcess> ShutdownBuildServers(RegiOptions options)
        {
            var output = new List<AppProcess>();

            output.Add(dotnetService.ShutdownBuildServer(options));

            return output.AsReadOnly();
        }

        private void LogOutputs(string stdout, string stderr, RegiOptions options)
        {
            if (console != null && options.Verbose)
            {
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    console.WriteDefaultLine(stdout);
                }
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    console.WriteErrorLine(stderr);
                }
            }
        }
    }
}
