﻿using McMaster.Extensions.CommandLineUtils;

namespace Regi.Models
{
    public class CommandOptions
    {
        [Argument(0, Description = "Name of the project")]
        public string Name { get; set; }

        [Option(Description = "Search pattern")]
        public string SearchPattern { get; set; }

        [Option(Description = "Project type")]
        public ProjectType? Type { get; set; }
    }
}