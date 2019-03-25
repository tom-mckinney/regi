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
        /// Initializes variable list from only specified projects
        /// </summary>
        /// <param name="projects"></param>
        public VariableList(IList<Project> projects) : base()
        {
            if (projects == null)
            {
                throw new NullReferenceException("Project list cannot be null when creating VariableList");
            }

            foreach (var project in projects)
            {
                AddProject(project);
            }
        }

        /// <summary>
        /// Initializes variable list from projects and all required projects
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="config"></param>
        public VariableList(IList<Project> projects, StartupConfig config) : base()
        {
            if (projects == null)
            {
                throw new NullReferenceException("Project list cannot be null when creating VariableList");
            }

            foreach (var project in projects)
            {
                AddProject(project);

                if (project.Requires?.Count > 0)
                {
                    foreach (var r in project.Requires)
                    {
                        Project requiredProject = config.Apps
                            .Concat(config.Services)
                            .FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));

                        if (requiredProject != null)
                        {
                            AddProject(requiredProject);
                        }
                    }
                }
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
