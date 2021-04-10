using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Services;
using System;
using System.Collections.Generic;
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

        public override bool RequireRegiConfig => false;

        protected override Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects => (c) => c.Projects;

        protected override async Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken)
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
