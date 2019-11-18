﻿using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services.Frameworks;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface ICleanupService
    {
        void KillProcessTree(AppProcess process, RegiOptions options);
        void KillProcessTree(AppProcess process, RegiOptions options, TimeSpan timeout);
        Task<IReadOnlyList<AppProcess>> ShutdownBuildServers(RegiOptions options, CancellationToken cancellationToken);
    }

    public class CleanupService : ICleanupService
    {
        private readonly IDotnetService _dotnetService;
        private readonly IFileSystem _fileSystem;
        private readonly IConsole _console;
        private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);

        public CleanupService(IDotnetService dotnetService, IFileSystem fileSystem, IConsole console)
        {
            _dotnetService = dotnetService;
            _fileSystem = fileSystem;
            _console = console;
        }

        public void KillProcessTree(AppProcess process, RegiOptions options)
        {
            KillProcessTree(process, options, _defaultTimeout);
        }

        public void KillProcessTree(AppProcess process, RegiOptions options, TimeSpan timeout)
        {
            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            string stdout;
            string stderr;

            if (_isWindows)
            {
                ProcessUtility.KillProcessWindows(process.ProcessId, _fileSystem, out stdout, out stderr);
                LogOutputs(stdout, stderr, options);
            }
            else
            {
                var children = new HashSet<int>();
                ProcessUtility.GetAllChildIdsUnix(process.ProcessId, children, _fileSystem);
                foreach (var childId in children)
                {
                    ProcessUtility.KillProcessUnix(childId, _fileSystem, out stdout, out stderr);
                    LogOutputs(stdout, stderr, options);
                }

                ProcessUtility.KillProcessUnix(process.ProcessId, _fileSystem, out stdout, out stderr);
                LogOutputs(stdout, stderr, options);
            }

            process.Kill(timeout, _console);
        }

        public async Task<IReadOnlyList<AppProcess>> ShutdownBuildServers(RegiOptions options, CancellationToken cancellationToken)
        {
            var output = new List<AppProcess>
            {
                await _dotnetService.ShutdownBuildServer(options, cancellationToken)
            };

            return output.AsReadOnly();
        }

        private void LogOutputs(string stdout, string stderr, RegiOptions options)
        {
            if (_console != null && options.Verbose)
            {
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    _console.WriteDefaultLine(stdout);
                }
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    _console.WriteErrorLine(stderr);
                }
            }
        }
    }
}
