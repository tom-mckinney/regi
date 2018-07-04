using System;
using System.ComponentModel.DataAnnotations;
using Regiment.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Regiment.Services;

namespace Regiment
{
    //[Command(Description = "My global command line tool.")]
    [Subcommand("unit", typeof(UnitCommand))]
    public class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        public static int MainWithConsole(IConsole console, string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IFileService, FileService>()
                .AddSingleton(console)
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            return app.Execute(args);
        }
    }
}
