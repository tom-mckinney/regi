using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Regi.Commands
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

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Tests;

        protected override int Execute(IList<Project> projects)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            _runnerService.Test(projects, Options);

            stopwatch.Stop();

            var output = _summaryService.PrintTestSummary(projects, stopwatch.Elapsed);

            return output.FailCount;
        }
    }
}
