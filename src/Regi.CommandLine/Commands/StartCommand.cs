using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Regi.Abstractions;
using Regi.Abstractions.Options;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.CommandLine.Commands
{
    [Command("start", AllowArgumentSeparator = true)]
    public class StartCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;
        private readonly IServiceRunnerDispatcher _serviceRunnerDispatcher;
        private readonly Settings _options;

        public StartCommand(
            IRunnerService runnerService,
            IServiceRunnerDispatcher serviceRunnerDispatcher,
            IProjectManager projectManager,
            IConfigurationService configurationService,
            IConsole console,
            IOptions<Settings> options)
            : base(projectManager, configurationService, console)
        {
            _runnerService = runnerService;
            _serviceRunnerDispatcher = serviceRunnerDispatcher;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects => (c) => c.Projects.WhereApp();

        protected override async Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken)
        {
            //await _runnerService.StartAsync(projects, Options, cancellationToken); // TODO: re-enable this after troubleshooting

            foreach (var service in ServiceMesh.Services)
            {
                await _serviceRunnerDispatcher.DispatchAsync(service, new OptionsBase(), cancellationToken);
            }

            while (_options.RunIndefinitely && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(-1, cancellationToken);
            }

            return 0;
        }
    }
}
