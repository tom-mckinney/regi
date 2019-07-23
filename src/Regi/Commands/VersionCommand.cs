using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;

namespace Regi.Commands
{
    [Command("version")]
    public class VersionCommand : CommandBase
    {
        public VersionCommand(IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
        }

        public override bool RequireStartupConfig => false;
        public override bool FilterProjects => false;

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => throw new NotImplementedException();

        protected override int Execute(IList<Project> projects)
        {
            var version = typeof(Program).Assembly.GetName().Version;

            console.WriteEmphasizedLine($"Regi version: {version.Major}.{version.Minor}.{version.Build}");

            return 0;
        }
    }
}
