using Regi.Extensions;
using System;
using System.Collections.Generic;

namespace Regi.Models
{
    public class VariableList : Dictionary<string, string>, IDictionary<string, string>
    {
        public VariableList() : base() { }

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

        public void AddProject(Project project)
        {
            if (project.Port.HasValue)
            {
                string underscoreName = project?.Name.ToUnderscoreCase();

                TryAdd($"{underscoreName}_PORT", project.Port.ToString());
                TryAdd($"{underscoreName}_URL", $"http://localhost:{project.Port}");
            }
        }
    }
}
