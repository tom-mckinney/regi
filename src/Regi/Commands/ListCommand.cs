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

        public ListCommand(IRunnerService runnerService)
            : base(null)
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
