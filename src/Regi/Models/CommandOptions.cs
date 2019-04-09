using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Regi.Models
{
    public class CommandOptions
    {
        [Argument(0, Description = "Name of the project")]
        public string Name { get; set; }

        [Argument(1, Name = "Additional arguments", Description = "Additional arguments to include in the framework-level command")]
        public string Arguments { get; set; }

        [Option(CommandOptionType.MultipleValue, Description = "Search pattern to exclude from the command")]
        public string[] Exclude { get; set; }

        [Option(Description = "Project type")]
        public ProjectType? Type { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Print all output")]
        public bool Verbose { get; set; } = false;

        [Option(CommandOptionType.SingleValue, Description = "Source to install dependencies from")]
        public string Source { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Do not run any processes in parallel")]
        public bool NoParallel { get; set; } = false;

        [Option(CommandOptionType.NoValue, Description = "Return raw output without any formatting or verbosity settings")]
        public bool RawOutput { get; set; } = false;

        public VariableList VariableList { get; set; }

        public bool KillProcessesOnExit { get; set; } = true;

        /// <summary>
        /// Clones options and sets Arguments to null. This is used when omitting arguments for required project.
        /// </summary>
        /// <returns></returns>
        public CommandOptions CloneForRequiredProjects()
        {
            CommandOptions clone = (CommandOptions)MemberwiseClone();

            clone.Arguments = null;

            return clone;
        }
    }
}
