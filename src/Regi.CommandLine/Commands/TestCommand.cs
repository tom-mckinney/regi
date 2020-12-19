using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Extensions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.CommandLine.Commands
{
    [Command("test", AllowArgumentSeparator = true)]
    public class TestCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;
        private readonly ISummaryService _summaryService;

        public TestCommand(IRunnerService runnerService, ISummaryService summaryService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
            : base(projectManager, configurationService, console)
        {
            _runnerService = runnerService;
            _summaryService = summaryService;
        }

        protected override Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects => (c) => c.Projects.WhereRoleIs(ProjectRole.Test);

        protected override async Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            await _runnerService.TestAsync(projects, Options, cancellationToken);

            stopwatch.Stop();

            var output = _summaryService.PrintTestSummary(projects, stopwatch.Elapsed);

            return output.FailCount;
        }
    }
}
