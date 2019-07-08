using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Regi.Commands
{
    [Command("start", AllowArgumentSeparator = true)]
    public class StartCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;
        private readonly Settings _options;

        public StartCommand(IRunnerService runnerService, IProjectManager projectManager, IConfigurationService configurationService, IConsole console, IOptions<Settings> options)
            : base(projectManager, configurationService, console)
        {
            _runnerService = runnerService;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (c) => c.Apps;

        protected override int Execute(IList<Project> projects)
        {
            _runnerService.Start(projects, Options);

            while (_options.RunIndefinitely)
            {
                Thread.Sleep(200);
            }

            return 0;
        }
    }
}
