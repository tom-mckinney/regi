using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Commands
{
    [Command("init")]
    public class InitalizeCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;

        public InitalizeCommand(IRunnerService runnerService, IConsole console)
            : base(console)
        {
            _runnerService = runnerService;
        }

        public override int OnExecute()
        {
            _runnerService.Initialize(Options);

            return 0;
        }
    }
}
