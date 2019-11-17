using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Regi.Models
{
    public class StartupConfig
    {
        [JsonIgnore]
        public string Path { get; set; }

        [JsonPropertyName("apps")]
        public List<Project> Apps { get; set; } = new List<Project>();

        [JsonPropertyName("tests")]
        public List<Project> Tests { get; set; } = new List<Project>();

        [JsonPropertyName("services")]
        public List<Project> Services { get; set; } = new List<Project>();

        // TODO: Remove RawSources when .NET 5 supports deserializing all Dictionary keys
        [JsonPropertyName("sources")]
        public IDictionary<string, string> RawSources { get; set; } = new Dictionary<string, string>();

        private Dictionary<ProjectFramework, string> _sources;

        [JsonIgnore]
        public IDictionary<ProjectFramework, string> Sources
        {
            get
            {
                if (_sources == null)
                {
                    _sources = new Dictionary<ProjectFramework, string>();

                    foreach (var source in RawSources)
                    {
                        var key = Enum.Parse<ProjectFramework>(source.Key);

                        _sources.Add(key, source.Value);
                    }
                }

                return _sources;
            }
        }
    }
}
