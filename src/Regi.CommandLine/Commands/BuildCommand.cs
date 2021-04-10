using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.CommandLine.Commands
{
    [Command("build")]
    public class BuildCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;

        public BuildCommand(IRunnerService runnerService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            _runnerService = runnerService;
        }

        protected override Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects => (c) => c.Projects;

        protected override async Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken)
        {
            await _runnerService.BuildAsync(projects, Options, cancellationToken);

            return 0;
        }
    }
}
