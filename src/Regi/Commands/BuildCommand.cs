using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Commands
{
    [Command("build")]
    public class BuildCommand : CommandBase
    {
        private IRunnerService _runnerService;

        public BuildCommand(IRunnerService runnerService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            _runnerService = runnerService;
        }

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Apps.Concat(c.Tests);

        protected override int Execute(IList<Project> projects)
        {
            _runnerService.Build(projects, Options);

            return 0;
        }
    }
}
