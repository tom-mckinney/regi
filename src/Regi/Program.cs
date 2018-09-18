using System;
using System.ComponentModel.DataAnnotations;
using Regi.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Regi.Services;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regi.Abstractions;
using Regi.Models;
using Regi.Extensions;

namespace Regi
{
    [Subcommand("start", typeof(StartCommand))]
    [Subcommand("test", typeof(TestCommand))]
    public class Program
    {
        public static int Main(string[] args) => MainWithConsole(PhysicalConsole.Singleton, args);

        public static int MainWithConsole(IConsole console, string[] args)
        {
            var services = new ServiceCollection()
                .Configure<Settings>(o =>
                {
                    o.RunIndefinitely = true;
                })
                .AddSingleton<IDotnetService, DotnetService>()
                .AddSingleton<INodeService, NodeService>()
                .AddSingleton<IRunnerService, RunnerService>()
                .AddSingleton<IFileService, FileService>()
                .AddSingleton(console)
                .AddSingleton<CommandLineContext, DefaultCommandLineContext>()
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            app.OnExecute(() => Main(new string[] { "--help" }));

            try
            {
                return app.Execute(args);
            }
            catch (Newtonsoft.Json.JsonSerializationException e)
            {
                // TODO: add custom handling for serialization exceptions
                console.WriteErrorLine(e.ToString());
                return 1;
            }
            catch (Exception e)
            {
                string failureMessage = e.InnerException?.Message ?? e.Message;

                console.WriteErrorLine(failureMessage);
                return 1;
            }
        }
    }
}
