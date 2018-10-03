using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Extensions
{
    public static class ListExtensions
    {
        public static IList<Project> FilterByOptions(this IList<Project> projects, CommandOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Name))
            {
                projects = projects
                    .Where(p => p.Name.Contains(options.Name, StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
            }


            if (options.Type.HasValue)
            {
                projects = projects
                    .Where(p => p.Type == options.Type)
                    .ToList();
            }

            return projects;
        }
    }
}
