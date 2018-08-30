using McMaster.Extensions.CommandLineUtils;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Commands
{
    [Command("start")]
    public class StartCommand
    {
        private readonly IRunnerService _runnerService;
        private readonly IConsole _console;

        public StartCommand(IRunnerService runnerService, IConsole console)
        {
            _runnerService = runnerService;
            _console = console;
        }

        [Argument(0, Description = "Name of the project")]
        public string Name { get; set; }

        public int OnExecute()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            var projects = _runnerService.RunAsync(currentDirectory);

            return projects.Count;
        }
    }
}
