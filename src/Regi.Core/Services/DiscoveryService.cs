using Regi.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IDiscoveryService
    {
        ValueTask<IEnumerable<IProject>> IdentifyAllProjectsAsync(DirectoryInfo directory);

        ValueTask<IProject> IdentifyProjectAsync(DirectoryInfo directory);
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

        public async ValueTask<IEnumerable<IProject>> IdentifyAllProjectsAsync(DirectoryInfo directory)
        {
            var project = await IdentifyProjectAsync(directory);

            if (project != null)
            {
                return new List<IProject> { project };
            }

            var output = new List<IProject>();

            foreach (var childDirectory in _fileSystem.GetChildDirectories(directory))
            {
                if (!Constants.DiscoverIgnore.Contains(childDirectory.Name))
                {
                    output.AddRange(await IdentifyAllProjectsAsync(childDirectory));
                }
            }

            return output;
        }

        public async ValueTask<IProject> IdentifyProjectAsync(DirectoryInfo directory)
        {
            IProject project = null;

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
