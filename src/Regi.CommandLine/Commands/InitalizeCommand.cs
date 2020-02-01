using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;

namespace Regi.CommandLine.Commands
{
    [Command("init")]
    public class InitalizeCommand : CommandBase
    {
        private readonly IDiscoveryService _discoveryService;
        private readonly IFileSystem _fileSystem;

        public InitalizeCommand(IDiscoveryService discoveryService,
            IFileSystem fileSystem,
            IProjectManager projectManager,
            IConfigurationService configurationService,
            IConsole console)
            : base(projectManager, configurationService, console)
        {
            _discoveryService = discoveryService;
            _fileSystem = fileSystem;
        }

        public override bool RequireStartupConfig => false;
        public override bool FilterProjects => false;

        protected override Func<StartupConfig, IEnumerable<Project>> GetTargetProjects => (s) => new List<Project>();

        protected override async Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken)
        {
            var directory = new DirectoryInfo(_fileSystem.WorkingDirectory);

            var existingProjects = await _discoveryService.IdentifyAllProjectsAsync(directory);

            var config = await _configurationService.CreateConfigurationAsync(existingProjects, Options);

            await _fileSystem.CreateConfigFileAsync(config);

            return 0;
        }
    }
}
