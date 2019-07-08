using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Commands
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

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Apps.Concat(c.Tests);

        protected override int Execute(IList<Project> projects)
        {
            try
            {
                _runnerService.Kill(projects, Options);
                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
}
