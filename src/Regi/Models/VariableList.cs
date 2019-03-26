using Regi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Models
{
    public class VariableList : Dictionary<string, string>, IDictionary<string, string>
    {
        public VariableList() : base() { }

        /// <summary>
        /// Initializes variable list from all apps and services
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="config"></param>
        public VariableList(StartupConfig config) : base()
        {
            if (config == null)
            {
                throw new ArgumentException("Project list cannot be null when creating VariableList", nameof(config));
            }

            foreach (var project in config.Apps.Concat(config.Services))
            {
                AddProject(project);
            }
        }

        public void AddProject(Project project)
        {
            if (project.Environment?.Count > 0)
            {
                foreach (var env in project.Environment)
                {
                    TryAdd(env.Key, env.Value);
                }
            }

            if (project.Port.HasValue)
            {
                string underscoreName = project?.Name.ToUnderscoreCase();

                TryAdd($"{underscoreName}_PORT", project.Port.ToString());
                TryAdd($"{underscoreName}_URL", $"http://localhost:{project.Port}");
            }
        }
    }
}
