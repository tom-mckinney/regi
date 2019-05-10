using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System.Diagnostics;

namespace Regi.Commands
{
    [Command("test", AllowArgumentSeparator = true)]
    public class TestCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;
        private readonly ISummaryService _summaryService;
        private readonly IConsole _console;

        public TestCommand(IRunnerService runnerService, ISummaryService summaryService, IConsole console)
            : base(console)
        {
            _runnerService = runnerService;
            _summaryService = summaryService;
            _console = console;
        }

        public override int OnExecute()
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            Projects = _runnerService.Test(Options);

            stopwatch.Stop();

            _summaryService.PrintTestSummary(Projects, stopwatch.Elapsed);

            int projectCount = Projects.Count;

            foreach (var p in Projects)
            {
                p.Process.Dispose();
            }

            return Projects.Count;
        }
    }
}
