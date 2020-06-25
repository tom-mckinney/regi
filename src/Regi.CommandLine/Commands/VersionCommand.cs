using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;

namespace Regi.CommandLine.Commands
{
    [Command("version")]
    public class VersionCommand : CommandBase
    {
        public VersionCommand(IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
        }

        public override bool RequireRegiConfig => false;
        public override bool FilterProjects => false;

        protected override Func<RegiConfig, IEnumerable<Project>> GetTargetProjects => (s) => new List<Project>();

        protected override Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            var version = typeof(Program).Assembly.GetName().Version;

            _console.WriteEmphasizedLine($"Regi version: {version.Major}.{version.Minor}.{version.Build}");

            return Task.FromResult(0);
        }
    }
}
