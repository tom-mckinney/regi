using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Regi.Abstractions;
using Regi.Commands;
using Regi.Extensions;
using Regi.Models;
using Regi.Models.Exceptions;
using Regi.Services;
using Regi.Services.Frameworks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Regi
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
                var protoServer = Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<ProtoServerStartup>();
                    })
                    .Build();

                protoServer.RunAsync();

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

                var shutdownCts = new CancellationTokenSource(5000);
                await projectManager.KillAllProcesses(new RegiOptions(), shutdownCts.Token, true);
            }
        }

        public static IServiceProvider ConfigureServices(IConsole console)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var services = new ServiceCollection()
                .Configure<Settings>(o =>
                {
                    o.RunIndefinitely = true;
                })
                .AddScoped<IQueueService, QueueService>()
                .AddSingleton<IProjectManager, ProjectManager>()
                .AddSingleton<IConfigurationService, ConfigurationService>()
                .AddSingleton<IFrameworkServiceProvider, FrameworkServiceProvider>()
                .AddSingleton<IDotnetService, DotnetService>()
                .AddSingleton<INodeService, NodeService>()
                .AddSingleton<IRunnerService, RunnerService>()
                .AddSingleton<IFileService, FileService>()
                .AddSingleton<INetworkingService, NetworkingService>()
                .AddSingleton<IPlatformService, PlatformService>()
                .AddSingleton<IRuntimeInfo, RuntimeInfo>()
                .AddSingleton<ISummaryService, SummaryService>()
                .AddSingleton<ICleanupService, CleanupService>()
                .AddSingleton(console)
                .AddSingleton<CommandLineContext, DefaultCommandLineContext>();

            services.AddTransient<System.Diagnostics.DiagnosticListener>();
            services.AddRouting();
            services.AddGrpc();
            services.AddLogging();

            return services.BuildServiceProvider();
        }
    }
}
