using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Extensions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        protected override Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects => (s) => new List<IProject>();

        protected override Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken)
        {
            var version = typeof(Program).Assembly.GetName().Version;

            _console.WriteEmphasizedLine($"Regi version: {version.Major}.{version.Minor}.{version.Build}");

            return Task.FromResult(0);
        }
    }
}
