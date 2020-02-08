using Regi.Models;
using Regi.Services;
using System;
using System.Threading.Tasks;

namespace Regi.Frameworks.Identifiers
{
    public class NodeIdentifier : BaseIdentifier
    {
        public NodeIdentifier(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        public override ValueTask<bool> ShouldIdentify(Project project, IFileSystemDictionary directoryContents)
        {
            return new ValueTask<bool>(true);
        }

        public override ValueTask<bool> IsMatchAsync(Project project, IFileSystemDictionary directoryContents)
        {
            if (project?.Framework == ProjectFramework.Node)
            {
                return new ValueTask<bool>(true);
            }

            if (directoryContents?.Count > 0)
            {
                foreach (var path in directoryContents.Keys)
                {
                    if (path.Equals("package.json", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new ValueTask<bool>(true);
                    }
                }
            }

            return new ValueTask<bool>(false);
        }

        public override async ValueTask<Project> CreateOrModifyAsync(Project project, IFileSystemDictionary directoryContents)
        {
            project = await base.CreateOrModifyAsync(project, directoryContents);

            project.Framework = ProjectFramework.Node;

            // TODO: determine type

            return project;
        }
    }
}
