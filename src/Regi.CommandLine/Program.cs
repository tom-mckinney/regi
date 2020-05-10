using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Regi.Abstractions;
using Regi.CommandLine.Commands;
using Regi.Extensions;
using Regi.Frameworks;
using Regi.Frameworks.Identifiers;
using Regi.Models;
using Regi.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.CommandLine
{
    [Subcommand(typeof(StartCommand))]
    [Subcommand(typeof(TestCommand))]
    [Subcommand(typeof(BuildCommand))]
    [Subcommand(typeof(InstallCommand))]
    [Subcommand(typeof(InitalizeCommand))]
    [Subcommand(typeof(ListCommand))]
    [Subcommand(typeof(KillCommand))]
    [Subcommand(typeof(VersionCommand))]
    public class Program
    {
        public static Task<int> Main(string[] args) => MainWithConsole(PhysicalConsole.Singleton, args);

        public static async Task<int> MainWithConsole(IConsole console, string[] args)
        {
            var services = ConfigureServices(console);

            using var app = new CommandLineApplication<Program>();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            app.OnExecuteAsync(c => Main(new string[] { "--help" }));

            try
            {
                return app.Execute(args);
            }
            catch (RegiException e)
            {
                return e.LogAndReturnStatus(console);
            }
            catch (CommandParsingException e)
            {
                return e.LogAndReturnStatus(console);
            }
            catch (Exception e)
            {
                return e.LogAllDetailsAndReturnStatus(console);
            }
            finally
            {
                var projectManager = app.GetRequiredService<IProjectManager>();

                using var shutdownCts = new CancellationTokenSource(5000);
                await projectManager.KillAllProcesses(new RegiOptions(), shutdownCts.Token, true);
            }
        }

        public static IServiceProvider ConfigureServices(IConsole console)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            return new ServiceCollection()
                .Configure<Settings>(o =>
                {
                    o.RunIndefinitely = true;
                })
                .AddScoped<IQueueService, QueueService>()
                .AddSingleton<IProjectManager, ProjectManager>()
                .AddSingleton<IConfigurationService, ConfigurationService>()
                .AddSingleton<IFrameworkServiceProvider, FrameworkServiceProvider>()
                .AddSingleton<IRunnerService, RunnerService>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<INetworkingService, NetworkingService>()
                .AddSingleton<IPlatformService, PlatformService>()
                .AddSingleton<IRuntimeInfo, RuntimeInfo>()
                .AddSingleton<ISummaryService, SummaryService>()
                .AddSingleton<ICleanupService, CleanupService>()
                .AddSingleton<IDiscoveryService, DiscoveryService>()

                .AddFramework<IDotnet, Dotnet>()
                .AddIdentifer<DotnetIdentifier>()

                .AddFramework<INode, Node>()
                .AddIdentifer<NodeIdentifier>()

                .AddFramework<IRubyOnRails, RubyOnRails>()

                .AddFramework<IDjango, Django>()
                .AddIdentifer<DjangoIdentifier>()

                .AddSingleton(console)
                .AddSingleton<CommandLineContext, DefaultCommandLineContext>()
                .BuildServiceProvider();
        }
    }
}
