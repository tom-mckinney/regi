using System;
using System.Collections.Generic;
using System.Text;

namespace Regiment.Models
{
    public class StartupConfig
    {
        public List<Project> Apps { get; set; }

        public List<Project> Tests { get; set; }

        public List<Project> Services { get; set; }
    }

    public enum ProjectType
    {
        Web = 1,
        Test,
        IntegrationTest
    }

    public class Project
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public ProjectType Type { get; set; }
    }
}
