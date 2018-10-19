using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Models
{
    public class StartupConfig
    {
        [JsonProperty("apps")]
        public List<Project> Apps { get; set; }

        [JsonProperty("tests")]
        public List<Project> Tests { get; set; }

        [JsonProperty("services")]
        public List<Project> Services { get; set; }

        public IList<Project> GetRequirements(Project project)
        {
            var requirements = from r in project.Requires
                               select Apps.Concat(Services)
                               .FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));
            return requirements.ToList();
        }
    }
}
