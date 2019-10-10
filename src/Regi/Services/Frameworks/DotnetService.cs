using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Services.Frameworks
{
    public interface IDotnetService : IFrameworkService
    {
        AppProcess ShutdownBuildServer(RegiOptions options);
    }

    public class DotnetService : FrameworkService, IDotnetService
    {
        public DotnetService(IConsole console, IPlatformService platformService) : base(console, platformService, DotNetExe.FullPathOrDefault())
        {
        }

        protected override CommandDictionary FrameworkOptions { get; } = new CommandDictionary
        {
            {
                FrameworkCommands.Dotnet.Run, new List<string>
                {
                    "--no-launch-profile"
                }
            }
        };

        protected override IList<string> FrameworkCommandWildcardExclusions => new List<string>
        {
            FrameworkCommands.Dotnet.Restore,
            FrameworkCommands.Dotnet.Build
        };

        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, RegiOptions options)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(project.Source))
                {
                    if (command == FrameworkCommands.Dotnet.Restore || command == FrameworkCommands.Dotnet.Run || command == FrameworkCommands.Dotnet.Build || command == FrameworkCommands.Dotnet.Publish)
                    {
                        builder.AppendCliOption($"--source {project.Source}");
                    }
                }

                base.ApplyFrameworkOptions(builder, command, project, options);
            }
        }

        protected override void SetEnvironmentVariables(Process process, Project project)
        {
            base.SetEnvironmentVariables(process, project);

            process.StartInfo.EnvironmentVariables.TryAdd("ASPNETCORE_ENVIRONMENT", "Development");

            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables["ASPNETCORE_URLS"] = $"http://*:{project.Port}"; // Default .NET Core URL variable
            }
        }

        public override async Task<AppProcess> InstallProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            AppProcess install = CreateProcess(FrameworkCommands.Dotnet.Restore, project, appDirectoryPath, options);

            install.Start();

            await install.WaitForExitAsync(cancellationToken);

            return install;
        }

        public override Task<AppProcess> StartProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                AppProcess start = CreateProcess(FrameworkCommands.Dotnet.Run, project, appDirectoryPath, options);

                start.Start();

                return start;
            }, cancellationToken);
        }

        public override async Task<AppProcess> TestProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            AppProcess test = CreateProcess(FrameworkCommands.Dotnet.Test, project, appDirectoryPath, options);

            test.Start();

            await test.WaitForExitAsync(cancellationToken);

            return test;
        }

        public override async Task<AppProcess> BuildProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            AppProcess build = CreateProcess(FrameworkCommands.Dotnet.Build, project, appDirectoryPath, options);

            build.Start();

            await build.WaitForExitAsync(cancellationToken);

            return build;
        }

        public override async Task<AppProcess> KillProcesses(RegiOptions options, CancellationToken cancellationToken)
        {
            AppProcess process = new AppProcess(_platformService.GetKillProcess("dotnet", options),
                AppTask.Kill,
                AppStatus.Running);

            try
            {
                if (options.KillProcessesOnExit)
                {
                    process.Start();
                    await process.WaitForExitAsync(cancellationToken);
                }

                process.Status = AppStatus.Success;
            }
            catch
            {
                process.Status = AppStatus.Failure;
            }

            return process;
        }

        public AppProcess ShutdownBuildServer(RegiOptions options)
        {
            AppProcess shutdownBuildServer = CreateProcess(FrameworkCommands.Dotnet.ShutdownBuildServer, options);

            shutdownBuildServer.Start();
            shutdownBuildServer.WaitForExit();

            return shutdownBuildServer;
        }
    }
}
