using Regi.Abstractions;
using Regi.Extensions;
using System;
using System.Threading.Tasks;

namespace Regi.Frameworks.Identifiers
{
    public class DotnetIdentifier : BaseIdentifier
    {
        private int Port = 8080;

        public DotnetIdentifier(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        public override ValueTask<bool> ShouldIdentify(IProject project, IFileSystemDictionary directoryContents)
        {
            return new ValueTask<bool>(true);
        }

        public override ValueTask<bool> IsMatchAsync(IProject project, IFileSystemDictionary directoryContents)
        {
            if (project?.Framework == ProjectFramework.Dotnet)
            {
                return new ValueTask<bool>(true);
            }

            if (directoryContents?.Count > 0)
            {
                foreach (var path in directoryContents.Keys)
                {
                    if (path.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase))
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

            project.Framework = ProjectFramework.Dotnet;

            if (project.Name.Contains("test", StringComparison.InvariantCultureIgnoreCase))
            {
                project.Roles.TryAdd(ProjectRole.Test);
            }
            else
            {
                project.Roles.TryAdd(ProjectRole.App);
                project.Port = Port;
            }

            Port++;

            return project;
        }
    }
}
