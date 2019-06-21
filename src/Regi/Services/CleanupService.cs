using Regi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Services
{
    public interface ICleanupService
    {
        IReadOnlyList<AppProcess> ShutdownBuildServers(RegiOptions options);
    }

    public class CleanupService : ICleanupService
    {
        private IDotnetService dotnetService;

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
