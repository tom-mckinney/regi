using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;

namespace Regi.Commands
{
    [Command("unit")]
    public class TestCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;
        private readonly IConsole _console;

        public TestCommand(IRunnerService runnerService, IConsole console)
        {
            _runnerService = runnerService;
            _console = console;
        }

        [Option(Description = "Type of tests to run", ShortName = "t", LongName = "type")]
        public ProjectType? Type { get; set; }

        public override int OnExecute()
        {
            var unitTests = _runnerService.Test(Type);

            return unitTests.Count;
        }
    }
}
