using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.CommandLine.Commands
{
    [Command("list")]
    public class ListCommand : CommandBase
    {
        private readonly ISummaryService _summaryService;

        public ListCommand(ISummaryService summaryService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            _summaryService = summaryService;
        }

        protected override Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects => (c) => new List<IProject>();

        protected override Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken)
        {
            _summaryService.PrintDomainSummary(ServiceMesh, Options);

            return Task.FromResult(0);
        }
    }
}
