using McMaster.Extensions.CommandLineUtils;

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
        public bool Verbose { get; set; }

        public VariableList VariableList { get; set; }

        public bool KillProcessesOnExit { get; set; } = true;
    }
}
