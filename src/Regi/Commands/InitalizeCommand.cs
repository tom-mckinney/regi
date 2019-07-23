using System;
using System.Collections.Generic;
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

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => throw new NotImplementedException();

        protected override int Execute(IList<Project> projects)
        {
            fileService.CreateConfigFile();

            return 0;
        }
    }
}
