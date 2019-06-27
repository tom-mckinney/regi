using McMaster.Extensions.CommandLineUtils;
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
        IReadOnlyList<AppProcess> ShutdownBuildServers(RegiOptions options);
    }

    public class CleanupService : ICleanupService
    {
        private readonly IDotnetService dotnetService;
        private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);

        public CleanupService(IDotnetService dotnetService)
        {
            this.dotnetService = dotnetService;
        }

        public IReadOnlyList<AppProcess> ShutdownBuildServers(RegiOptions options)
        {
            var output = new List<AppProcess>();

            output.Add(dotnetService.ShutdownBuildServer(options));

            return output.AsReadOnly();
        }
    }
}
