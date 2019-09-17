using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Commands
{
    [Command("install", AllowArgumentSeparator = true)]
    public class InstallCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;

        public InstallCommand(IRunnerService runnerService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            _runnerService = runnerService;
        }

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Apps.Concat(c.Tests);

        protected override async Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            await _runnerService.InstallAsync(projects, Options, cancellationToken);

            return projects
                .Where(p => p.Processes?.Any(x => x.Status == AppStatus.Failure) == true)
                .Count();
        }
    }
}
