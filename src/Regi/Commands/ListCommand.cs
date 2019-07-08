using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;

namespace Regi.Commands
{
    [Command("list")]
    public class ListCommand : CommandBase
    {
        private ISummaryService summaryService;

        public ListCommand(ISummaryService summaryService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            this.summaryService = summaryService;
        }

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => new List<Project>();

        protected override int Execute(IList<Project> projects)
        {
            summaryService.PrintDomainSummary(Config, Options);

            return 0;
        }
    }
}
