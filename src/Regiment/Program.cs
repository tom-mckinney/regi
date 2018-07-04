using System;
using System.ComponentModel.DataAnnotations;
using Regiment.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace Regiment
{
    //[Command(Description = "My global command line tool.")]
    [Subcommand("unit", typeof(UnitCommand))]
    public class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        public static int MainWithConsole(IConsole console, string[] args) => CommandLineApplication.Execute<Program>(console, args);
    }
}
