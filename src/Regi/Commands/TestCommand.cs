using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regi.Models;
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
    public class TestCommand
    {
        private readonly IRunnerService _runnerService;
        private readonly IConsole _console;

        public TestCommand(IRunnerService runnerService, IConsole console)
        {
            _runnerService = runnerService;
            _console = console;
        }

        [Argument(0, Description = "Name of the project")]
        public string Name { get; set; }

        [Option(Description = "Type of tests to run", ShortName = "t", LongName = "type")]
        public ProjectType? Type { get; set; }

        public int OnExecute()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

             var unitTests = _runnerService.TestAsync(currentDirectory, Type);

            return unitTests.Count;
        }
    }
}
