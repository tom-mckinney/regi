using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;

namespace Regi.Commands
{
    [Command("init")]
    public class InitalizeCommand : CommandBase
    {
        private readonly IFileService fileService;

        public InitalizeCommand(IFileService fileService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            this.fileService = fileService;
        }

        public override bool RequireStartupConfig => false;
        public override bool FilterProjects => false;

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (s) => new List<Project>();

        protected override Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            fileService.CreateConfigFile();

            return Task.FromResult(0);
        }
    }
}
