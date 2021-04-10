using Regi.Abstractions;
using Regi.Models;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Extensions
{
    public static class ProjectExtensions
    {
        public static void TryAddSource(this IProject project, CommandOptions options, IServiceMesh config)
        {
            if (project == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(project.Source))
            {
                if (!string.IsNullOrWhiteSpace(options.Source))
                {
                    project.Source = options.Source;
                }
                else if (config.GetSources()?.Count > 0
                    && (config.GetSources().TryGetValue(project.Framework, out string source) || config.GetSources().TryGetValue(ProjectFramework.Any, out source))
                    && !string.IsNullOrWhiteSpace(source))
                {
                    project.Source = source;
                }
            }
        }

        public static IEnumerable<IProject> WhereRoleIs(
            this IEnumerable<IProject> projects,
            params ProjectRole[] roles)
        {
            return projects.Where(p => p.Roles.ContainsAny(roles));
        }

        public static IEnumerable<IProject> WhereApp(this IEnumerable<IProject> projects)
        {
            return WhereRoleIs(projects, ProjectRole.App);
        }

        public static IEnumerable<IProject> WhereTest(this IEnumerable<IProject> projects)
        {
            return WhereRoleIs(projects, ProjectRole.Test);
        }
    }
}
