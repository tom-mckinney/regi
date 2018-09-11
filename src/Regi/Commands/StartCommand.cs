using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Regi.Extensions;
using Regi.Models;
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
        private readonly Settings _options;

        public StartCommand(IRunnerService runnerService, IConsole console, IOptions<Settings> options)
        {
            _runnerService = runnerService;
            _console = console;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        [Argument(0, Description = "Name of the project")]
        public string Name { get; set; }

        public int OnExecute()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            var projects = _runnerService.RunAsync(currentDirectory);

            // TODO: Make this wait configurable
            while (_options.RunIndefinitely)
            {
                var key = System.Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    foreach (var p in projects)
                    {
                        p.Process.KillTree(TimeSpan.FromSeconds(10));
                    }
                    break;
                }
            }

            return projects.Count;
        }
    }
}
