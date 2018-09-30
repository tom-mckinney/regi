using Newtonsoft.Json;
using System.Collections.Generic;

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
    }
}
