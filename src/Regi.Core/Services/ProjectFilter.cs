using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Regi.Services
{
    public interface IProjectFilter
    {
        IEnumerable<Project> FilterByName(IEnumerable<Project> projects, string namePattern);
        IEnumerable<Project> FilterByLabels(IEnumerable<Project> projects, IEnumerable<string> labelPatterns);
        IEnumerable<Project> FilterByExclusions(IEnumerable<Project> projects, IEnumerable<string> exclusionPatterns);
        IEnumerable<Project> FilterByRoles(IEnumerable<Project> projects, IEnumerable<ProjectRole> projectRoles);
        IEnumerable<Project> FilterByOptional(IEnumerable<Project> projects);
    }

    public class ProjectFilter : IProjectFilter
    {
        public IEnumerable<Project> FilterByName(IEnumerable<Project> projects, string namePattern)
        {
            return projects.Where(p => Regex.IsMatch(p.Name, namePattern, RegexOptions.IgnoreCase));
        }

        public IEnumerable<Project> FilterByLabels(IEnumerable<Project> projects, IEnumerable<string> labelPatterns)
        {
            return projects.Where(p =>
                    p.Labels.Any(label =>
                        labelPatterns.Any(pattern => Regex.IsMatch(label, pattern, RegexOptions.IgnoreCase))));
        }

        public IEnumerable<Project> FilterByExclusions(IEnumerable<Project> projects, IEnumerable<string> exclusionPatterns)
        {
            return projects.Where(p => 
                !exclusionPatterns.Any(e => Regex.IsMatch(p.Name, e, RegexOptions.IgnoreCase)));
        }

        public IEnumerable<Project> FilterByRoles(IEnumerable<Project> projects, IEnumerable<ProjectRole> projectRoles)
        {
            return projects.Where(p => p.Roles.ContainsAny(projectRoles));
        }

        public IEnumerable<Project> FilterByOptional(IEnumerable<Project> projects)
        {
            return projects.Where(p => p.Optional == false);
        }
    }
}
