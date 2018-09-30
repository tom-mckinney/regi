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
    public class StartCommand : CommandBase
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

        public override int OnExecute()
        {
            var projects = _runnerService.Run();

            // TODO: Make this wait configurable
            while (_options.RunIndefinitely)
            {
                var key = Console.ReadKey(true);
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
