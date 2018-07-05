using System;
using System.ComponentModel.DataAnnotations;
using Regiment.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Regiment.Services;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regiment.Abstractions;

namespace Regiment
{
    [Subcommand("unit", typeof(UnitCommand))]
    public class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        public static int MainWithConsole(IConsole console, string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IFileService, FileService>()
                .AddSingleton(console)
                .AddSingleton<CommandLineContext, DefaultCommandLineContext>()
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            return app.Execute(args);
        }
    }
}
