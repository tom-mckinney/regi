using McMaster.Extensions.CommandLineUtils;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Commands
{
    [Command("build")]
    public class BuildCommand : CommandBase
    {
        private IRunnerService _runnerService;

        public BuildCommand(IRunnerService runnerService, IConsole console)
            : base(console)
        {
            _runnerService = runnerService;
        }

        public override int OnExecute()
        {
            _runnerService.Build(Options);

            return 0;
        }
    }
}
