using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override Func<RegiConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Projects;

        protected override async Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            await _runnerService.BuildAsync(projects, Options, cancellationToken);

            return 0;
        }
    }
}
