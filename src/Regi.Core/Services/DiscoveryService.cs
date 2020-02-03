using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IDiscoveryService
    {
        ValueTask<IEnumerable<Project>> IdentifyAllProjectsAsync(DirectoryInfo directory);

        ValueTask<Project> IdentifyProjectAsync(DirectoryInfo directory);
    }

    public class DiscoveryService : IDiscoveryService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IEnumerable<IIdentifier> _identifiers;

        public DiscoveryService(IFileSystem fileSystem, IEnumerable<IIdentifier> identifiers)
        {
            _fileSystem = fileSystem;
            _identifiers = identifiers;
        }

        public async ValueTask<IEnumerable<Project>> IdentifyAllProjectsAsync(DirectoryInfo directory)
        {
            var project = await IdentifyProjectAsync(directory);

            if (project != null)
            {
                return new List<Project> { project };
            }

            var output = new List<Project>();

            foreach (var childDirectory in _fileSystem.GetChildDirectories(directory))
            {
                if (!Constants.DiscoverIgnore.Contains(childDirectory.Name))
                {
                    output.AddRange(await IdentifyAllProjectsAsync(childDirectory));
                }
            }

            return output;
        }

        public async ValueTask<Project> IdentifyProjectAsync(DirectoryInfo directory)
        {
            Project project = null;

            var fileSystemObjects = _fileSystem.GetAllChildren(directory);

            foreach (var identifier in _identifiers)
            {
                if (await identifier.ShouldIdentify(project, fileSystemObjects))
                {
                    if (await identifier.IsMatchAsync(project, fileSystemObjects))
                    {
                        project = await identifier.CreateOrModifyAsync(project, fileSystemObjects);
                    }
                }
            }

            return project;
        }
    }
}
