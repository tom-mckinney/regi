using Regi.Abstractions;
using Regi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Regi.Frameworks.Identifiers
{
    public abstract class BaseIdentifier : IIdentifier
    {
        private readonly IFileSystem _fileSystem;

        protected BaseIdentifier(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public abstract ValueTask<bool> IsMatchAsync(IProject project, IFileSystemDictionary directoryContents);
        public abstract ValueTask<bool> ShouldIdentify(IProject project, IFileSystemDictionary directoryContents);
        
        public virtual ValueTask<IProject> CreateOrModifyAsync(IProject project, IFileSystemDictionary directoryContents)
        {
            if (project == null)
            {
                project = new Project();
            }

            project.Name = directoryContents.Name;
            project.Paths = new List<string>
            {
                _fileSystem.GetRelativePath(directoryContents.Path)
            };
            project.Roles = new List<ProjectRole>(); // do not default this during identification

            return new ValueTask<IProject>(project);
        }
    }
}
