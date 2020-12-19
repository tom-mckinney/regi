using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.CommandLine.Commands
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

        protected override Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects => (c) => c.Projects;

        protected override async Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken)
        {
            await _runnerService.InstallAsync(projects, Options, cancellationToken);

            return projects
                .Count(p => p.Processes?.Any(x => x.Status == AppStatus.Failure) == true);
        }
    }
}
