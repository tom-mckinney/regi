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
    [Command("kill")]
    public class KillCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;

        public KillCommand(IRunnerService runnerService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            _runnerService = runnerService;
        }

        public override bool RequireStartupConfig => false;

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Projects;

        protected override async Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            try
            {
                await _runnerService.KillAsync(projects, Options, cancellationToken);

                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
}
