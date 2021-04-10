using Regi.Abstractions;
using Regi.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Regi.Services
{
    public interface IProjectFilter
    {
        IEnumerable<IProject> FilterByName(IEnumerable<IProject> projects, string namePattern);
        IEnumerable<IProject> FilterByLabels(IEnumerable<IProject> projects, IEnumerable<string> labelPatterns);
        IEnumerable<IProject> FilterByExclusions(IEnumerable<IProject> projects, IEnumerable<string> exclusionPatterns);
        IEnumerable<IProject> FilterByRoles(IEnumerable<IProject> projects, IEnumerable<ProjectRole> projectRoles);
        IEnumerable<IProject> FilterByOptional(IEnumerable<IProject> projects);
    }

    public class ProjectFilter : IProjectFilter
    {
        public IEnumerable<IProject> FilterByName(IEnumerable<IProject> projects, string namePattern)
        {
            return projects.Where(p => Regex.IsMatch(p.Name, namePattern, RegexOptions.IgnoreCase));
        }

        public IEnumerable<IProject> FilterByLabels(IEnumerable<IProject> projects, IEnumerable<string> labelPatterns)
        {
            return projects.Where(p =>
                    p.Labels.Any(label =>
                        labelPatterns.Any(pattern => Regex.IsMatch(label, pattern, RegexOptions.IgnoreCase))));
        }

        public IEnumerable<IProject> FilterByExclusions(IEnumerable<IProject> projects, IEnumerable<string> exclusionPatterns)
        {
            return projects.Where(p => 
                !exclusionPatterns.Any(e => Regex.IsMatch(p.Name, e, RegexOptions.IgnoreCase)));
        }

        public IEnumerable<IProject> FilterByRoles(IEnumerable<IProject> projects, IEnumerable<ProjectRole> projectRoles)
        {
            return projects.Where(p => p.Roles.ContainsAny(projectRoles));
        }

        public IEnumerable<IProject> FilterByOptional(IEnumerable<IProject> projects)
        {
            return projects.Where(p => !p.Optional);
        }
    }
}
