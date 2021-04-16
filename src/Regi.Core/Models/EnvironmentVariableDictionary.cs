using Regi.Abstractions;
using Regi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Models
{
    public class EnvironmentVariableDictionary : Dictionary<string, object>, IDictionary<string, object>
    {
        public EnvironmentVariableDictionary() : base() { }

        /// <summary>
        /// Initializes variable list from all apps, tests, and services
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="config"></param>
        public EnvironmentVariableDictionary(IServiceMesh config) : base()
        {
            if (config == null)
            {
                throw new ArgumentException("Project list cannot be null when creating VariableList", nameof(config));
            }

            var allProjects = config.Projects;
                //.Concat(config.Services); // TODO: figure this out

            foreach (var project in allProjects)
            {
                AddProject(project);
            }
        }

        public void AddProject(IProject project)
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
