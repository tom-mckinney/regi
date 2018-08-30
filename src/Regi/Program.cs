using System;
using System.ComponentModel.DataAnnotations;
using Regi.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Regi.Services;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regi.Abstractions;

namespace Regi
{
    [Subcommand("unit", typeof(UnitCommand))]
    public class Program
    {
        public static int Main(string[] args) => MainWithConsole(PhysicalConsole.Singleton, args);

        public static int MainWithConsole(IConsole console, string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IDotnetService, DotnetService>()
                .AddSingleton<IRunnerService, RunnerService>()
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
