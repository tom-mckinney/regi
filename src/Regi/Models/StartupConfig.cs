using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

    public enum ProjectType
    {
        Web = 1,
        Unit,
        Integration
    }

    public enum ProjectFramework
    {
        Dotnet = 1,
        Node
    }

    public class Project
    {
        public string Name { get; set; }

        public string Path { get; set; }

        [JsonProperty("type")]
        public ProjectType Type { get; set; }

        public ProjectFramework Framework { get; set; } = ProjectFramework.Dotnet;

        public int? Port { get; set; }
    }
}
