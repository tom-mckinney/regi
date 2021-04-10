using Regi.Abstractions;
using Regi.Extensions;
using System;
using System.Threading.Tasks;

namespace Regi.Frameworks.Identifiers
{
    public class DjangoIdentifier : BaseIdentifier
    {
        public DjangoIdentifier(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        public override ValueTask<bool> ShouldIdentify(IProject project, IFileSystemDictionary directoryContents)
        {
            return new ValueTask<bool>(true);
        }

        public override ValueTask<bool> IsMatchAsync(IProject project, IFileSystemDictionary directoryContents)
        {
            if (project?.Framework == ProjectFramework.Django)
            {
                return new ValueTask<bool>(true);
            }

            if (directoryContents?.Count > 0)
            {
                foreach (var path in directoryContents.Keys)
                {
                    if (path.Equals("manage.py", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new ValueTask<bool>(true);
                    }
                }
            }

            return new ValueTask<bool>(false);
        }

        public override async ValueTask<IProject> CreateOrModifyAsync(IProject project, IFileSystemDictionary directoryContents)
        {
            project = await base.CreateOrModifyAsync(project, directoryContents);

            project.Framework = ProjectFramework.Django;
            project.Roles.TryAdd(ProjectRole.App);

            return project;
        }
    }
}
