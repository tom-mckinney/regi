using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Services.Identifiers
{
    public abstract class BaseIdentifier : IIdentifier
    {
        private readonly IFileSystem _fileSystem;

        public BaseIdentifier(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public abstract ValueTask<bool> IsMatchAsync(Project project, IFileSystemDictionary directoryContents);
        public abstract ValueTask<bool> ShouldIdentify(Project project, IFileSystemDictionary directoryContents);
        
        public virtual ValueTask<Project> CreateOrModifyAsync(Project project, IFileSystemDictionary directoryContents)
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

            return new ValueTask<Project>(project);
        }
    }
}
