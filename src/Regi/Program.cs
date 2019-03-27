using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Regi.Abstractions;
using Regi.Commands;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;

namespace Regi
{
    [Subcommand(typeof(StartCommand))]
    [Subcommand(typeof(TestCommand))]
    [Subcommand(typeof(InstallCommand))]
    [Subcommand(typeof(InitalizeCommand))]
    [Subcommand(typeof(ListCommand))]
    public class Program
    {
        public static int Main(string[] args) => MainWithConsole(PhysicalConsole.Singleton, args);

        public static int MainWithConsole(IConsole console, string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            var services = new ServiceCollection()
                .Configure<Settings>(o =>
                {
                    o.RunIndefinitely = true;
                })
                .AddSingleton<IDotnetService, DotnetService>()
                .AddSingleton<INodeService, NodeService>()
                .AddSingleton<IRunnerService, RunnerService>()
                .AddSingleton<IFileService, FileService>()
                .AddTransient<IParallelService, ParallelService>()
                .AddTransient<INetworkingService, NetworkingService>()
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
            catch (JsonSerializationException e)
            {
                return e.LogAndReturnStatus(console);
            }
            catch (Exception e) when (e.InnerException is JsonSerializationException jsonException)
            {
                return jsonException.LogAndReturnStatus(console);
            }
            catch (Exception e)
            {
                return e.LogAndReturnStatus(console);
            }
        }
    }
}
