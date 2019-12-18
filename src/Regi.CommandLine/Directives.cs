using Regi.Models;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text;

namespace Regi.CommandLine
{
    public static class Directives
    {
        public static List<Argument> DefaultArguments => new List<Argument>
        {
            Name
        };

        public static List<Option> DefaultOptions => new List<Option>
        {
            ConfigurationPath,
            Type,
            Exclude,
        };

        public static Argument<string> Name => new Argument<string>("name")
        {
            Description = "Name of the project"
        };

        public static Option<FileSystemInfo> ConfigurationPath => new Option<FileSystemInfo>(
            new[] { "-c", "--configuration", "--configuration-path" },
            "Path to the configuration file")
        {
            Argument = new Argument<FileSystemInfo>("path")
        };

        public static Option<List<string>> Exclude => new Option<List<string>>(new[] { "-e", "--exclude" }, "Search pattern to exclude from the command");

        public static Option<ProjectType?> Type => new Option<ProjectType?>(new[] { "-t", "--type" }, "Project type");
    }
}
