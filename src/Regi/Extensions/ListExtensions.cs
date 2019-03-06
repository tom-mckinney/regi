using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Extensions
{
    public static class ListExtensions
    {
        public static IList<Project> FilterByOptions(this IEnumerable<Project> projects, CommandOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Name))
            {
                projects = projects
                    .Where(p => p.Name.Contains(options.Name, StringComparison.CurrentCultureIgnoreCase));
            }

            if (options.Exclude != null && options.Exclude.Any())
            {
                projects = projects
                    .Where(p =>
                    {
                        foreach (var exclusion in options.Exclude)
                        {
                            if (p.Name.Contains(exclusion, StringComparison.CurrentCultureIgnoreCase))
                                return false;
                        }

                        return true;
                    });
            }

            if (options.Type.HasValue)
            {
                projects = projects
                    .Where(p => p.Type == options.Type);
            }

            return projects.ToList();
        }
    }
}
