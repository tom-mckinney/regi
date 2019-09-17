using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Regi.Models
{
    public class RegiOptions
    {
        [Argument(0, Description = "Name of the project")]
        public string Name { get; set; }

        public List<string> RemainingArguments { get; set; }

        [Option(CommandOptionType.MultipleValue, Description = "Search pattern to exclude from the command")]
        public List<string> Exclude { get; set; }

        [Option(Description = "Project type")]
        public ProjectType? Type { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Print all output")]
        public bool Verbose { get; set; } = false;

        [Option(CommandOptionType.MultipleValue, Template = "-o --show-output", Description = "Show all output for given project or projects (verbose for individual project)")]
        public List<string> ShowOutput { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "Source to install dependencies from")]
        public string Source { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Do not run any processes in parallel")]
        public bool NoParallel { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Return raw output without any formatting or verbosity settings")]
        public bool RawOutput { get; set; } = false;

        public EnvironmentVariableDictionary VariableList { get; set; }

        public bool KillProcessesOnExit { get; set; } = true;

        /// <summary>
        /// Clones options and sets Arguments to null. This is used when omitting arguments or properties for required projects.
        /// </summary>
        /// <returns></returns>
        public RegiOptions CloneForRequiredProjects()
        {
            RegiOptions clone = (RegiOptions)MemberwiseClone();

            clone.RemainingArguments = null;

            return clone;
        }
    }
}
