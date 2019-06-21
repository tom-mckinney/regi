using McMaster.Extensions.CommandLineUtils;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Commands
{
    [Command("list")]
    public class ListCommand : CommandBase
    {
        private IRunnerService _runnerService;

        public ListCommand(IRunnerService runnerService, ICleanupService cleanupService, IConsole console)
            : base(cleanupService, console)
        {
            _runnerService = runnerService;
        }

        public override int OnExecute()
        {
            _runnerService.List(Options);

            return 0;
        }
    }
}
