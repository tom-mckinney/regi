using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regi.Extensions
{
    public static class ProjectExtensions
    {
        public static IEnumerable<Project> WhereRoleIs(
            this IEnumerable<Project> projects,
            params ProjectRole[] roles)
        {
            return projects.Where(p => p.Roles.ContainsAny(roles));
        }

        public static IEnumerable<Project> WhereApp(this IEnumerable<Project> projects)
        {
            return WhereRoleIs(projects, ProjectRole.Web);
        }

        public static IEnumerable<Project> WhereTest(this IEnumerable<Project> projects)
        {
            return WhereRoleIs(projects, ProjectRole.Unit, ProjectRole.Integration);
        }
    }
}
