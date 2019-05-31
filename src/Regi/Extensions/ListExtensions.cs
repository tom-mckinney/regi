using Regi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Regi.Extensions
{
    public static class ListExtensions
    {
        public static IList<Project> FilterByOptions(this IEnumerable<Project> projects, RegiOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Name))
            {
                projects = projects.Where(p => new Regex(options.Name, RegexOptions.IgnoreCase).IsMatch(p.Name));
                
            }

            if (options.Exclude != null && options.Exclude.Any())
            {
                projects = projects
                    .Where(p =>
                    {
                        foreach (var exclusion in options.Exclude)
                        {
                            if (new Regex(exclusion, RegexOptions.IgnoreCase).IsMatch(p.Name))
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

        public static void DisposeAll(this IEnumerable<AppProcess> processes)
        {
            if (processes?.Count() > 0)
            {
                foreach (var p in processes)
                {
                    p.Dispose();
                }
            }
        }
    }
}
