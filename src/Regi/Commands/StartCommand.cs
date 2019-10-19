using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        protected override async Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            var serverThread = new Thread(async () =>
            {
                var protoServer = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls("https://localhost:5051/");
                        webBuilder.UseStartup<ProtoServerStartup>();
                    })
                    .Build();

                await protoServer.RunAsync(cancellationToken);
            });

            serverThread.Start();

            await _runnerService.StartAsync(projects, Options, cancellationToken);

            while (_options.RunIndefinitely && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(200);
            }

            return 0;
        }
    }
}
