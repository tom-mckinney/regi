using McMaster.Extensions.CommandLineUtils;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Commands
{
    [Command("kill")]
    public class KillCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;
        private readonly IConsole _console;

        public KillCommand(IRunnerService runnerService, IConsole console)
            : base(console)
        {
            _runnerService = runnerService;
            _console = console;
        }

        public override int OnExecute()
        {
            try
            {
                _runnerService.Kill(Options);
                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
}
