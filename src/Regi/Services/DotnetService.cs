using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regi.Services
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

        protected override ProjectOptions FrameworkOptions { get; } = new ProjectOptions
        {
            {
                FrameworkCommands.Dotnet.Run, new List<string>
                {
                    "--no-launch-profile"
                }
            }
        };

        protected override IList<string> FrameworkCommandWildcardExclusions => new List<string> { FrameworkCommands.Dotnet.Restore };

        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, RegiOptions options)
        {
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

        public override AppProcess InstallProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            AppProcess install = CreateProcess(FrameworkCommands.Dotnet.Restore, project, appDirectoryPath, options);

            install.Start();

            install.WaitForExit();

            return install;
        }

        public override AppProcess StartProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            AppProcess start = CreateProcess(FrameworkCommands.Dotnet.Run, project, appDirectoryPath, options);

            start.Start();

            return start;
        }

        public override AppProcess TestProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            AppProcess test = CreateProcess(FrameworkCommands.Dotnet.Test, project, appDirectoryPath, options);

            test.Start();

            test.WaitForExit();

            return test;
        }

        public override AppProcess BuildProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            AppProcess build = CreateProcess(FrameworkCommands.Dotnet.Build, project, appDirectoryPath, options);

            build.Start();

            build.WaitForExit();

            return build;
        }

        public override AppProcess KillProcesses(RegiOptions options)
        {
            AppProcess process = new AppProcess(_platformService.GetKillProcess("dotnet", options),
                AppTask.Kill,
                AppStatus.Running);

            try
            {
                if (options.KillProcessesOnExit)
                {
                    process.Start();
                    process.WaitForExit();
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
