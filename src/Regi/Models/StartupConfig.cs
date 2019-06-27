using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Models
{
    public class StartupConfig
    {
        [JsonProperty("apps")]
        public List<Project> Apps { get; set; } = new List<Project>();

        [JsonProperty("tests")]
        public List<Project> Tests { get; set; } = new List<Project>();

        [JsonProperty("services")]
        public List<Project> Services { get; set; } = new List<Project>();

        [JsonProperty("sources")]
        public IDictionary<ProjectFramework, string> Sources { get; set; } = new Dictionary<ProjectFramework, string>();

        public IList<Project> GetRequirements(Project project)
        {
            var requirements = from r in project.Requires
                               select Apps.Concat(Services)
                               .FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));
            return requirements.ToList();
        }
    }
}
