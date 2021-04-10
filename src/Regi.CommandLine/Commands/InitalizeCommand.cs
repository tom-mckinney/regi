using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        public override bool RequireRegiConfig => false;
        public override bool FilterProjects => false;

        protected override Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects => (s) => new List<IProject>();

        protected override async Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken)
        {
            var directory = new DirectoryInfo(_fileSystem.WorkingDirectory);

            var existingProjects = await _discoveryService.IdentifyAllProjectsAsync(directory);

            var config = await _configurationService.CreateConfigurationAsync(existingProjects, Options);

            await _fileSystem.CreateConfigFileAsync(config);

            return 0;
        }
    }
}
