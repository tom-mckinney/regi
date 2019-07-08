using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Commands
{
    [Command("install")]
    public class InstallCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;

        public InstallCommand(IRunnerService runnerService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            _runnerService = runnerService;
        }

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Apps.Concat(c.Tests);

        protected override int Execute(IList<Project> projects)
        {
            _runnerService.Install(projects, Options);

            return projects
                .Where(p => p.Processes?.Any(x => x.Status == AppStatus.Failure) == true)
                .Count();
        }
    }
}
