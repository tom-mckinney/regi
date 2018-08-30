using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Regi.Commands
{
    [Command("unit")]
    public class UnitCommand
    {
        private readonly IRunnerService _runnerService;

        public UnitCommand(IRunnerService runnerService)
        {
            _runnerService = runnerService;
        }

        [Argument(0, Description = "Name of the project")]
        public string Name { get; set; }

        public int OnExecute()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

             var unitTests = _runnerService.TestAsync(currentDirectory);

            return unitTests.Count;
        }
    }
}
