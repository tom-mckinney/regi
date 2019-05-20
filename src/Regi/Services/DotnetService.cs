using McMaster.Extensions.CommandLineUtils;
using Regi.Constants;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regi.Services
{
    public interface IDotnetService : IFrameworkService
    {
    }

    public class DotnetService : FrameworkService, IDotnetService
    {
        public DotnetService(IConsole console, IPlatformService platformService) : base(console, platformService, DotNetExe.FullPathOrDefault())
        {
        }

        protected override ProjectOptions FrameworkOptions { get; } = new ProjectOptions
        {
            { FrameworkCommands.Dotnet.Run, new List<string> { "--no-launch-profile" } }
        };

        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, RegiOptions options)
        {
            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(project.Source))
                {
                    FrameworkOptions.AddOptions(FrameworkCommands.Any, $"--source {project.Source}");
                }

                base.ApplyFrameworkOptions(builder, command, project, options);
            }
        }

        protected override void SetEnvironmentVariables(Process process, Project project)
        {
            base.SetEnvironmentVariables(process, project);

            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            process.StartInfo.EnvironmentVariables.Add("ASPNETCORE_ENVIRONMENT", environmentName);

            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables["ASPNETCORE_URLS"] = $"http://*:{project.Port}"; // Default .NET Core URL variable
                //process.StartInfo.EnvironmentVariables.Add("ASPNETCORE_URLS", $"http://*:{project.Port}"); // Default .NET Core URL variable
            }
        }

        public override AppProcess InstallProject(Project project, RegiOptions options)
        {
            AppProcess install = CreateProcess(FrameworkCommands.Dotnet.Restore, project, options);

            install.Start();

            install.WaitForExit();

            return install;
        }

        public override AppProcess StartProject(Project project, RegiOptions options)
        {
            AppProcess start = CreateProcess(FrameworkCommands.Dotnet.Run, project, options);

            start.Start();

            return start;
        }

        public override AppProcess TestProject(Project project, RegiOptions options)
        {
            AppProcess test = CreateProcess(FrameworkCommands.Dotnet.Test, project, options);

            test.Start();

            test.WaitForExit();

            return test;
        }

        public override AppProcess BuildProject(Project project, RegiOptions options)
        {
            AppProcess build = CreateProcess(FrameworkCommands.Dotnet.Build, project, options);

            build.Start();

            build.WaitForExit();

            return build;
        }

        public override AppProcess KillProcesses(RegiOptions options)
        {
            AppProcess process = new AppProcess(_platformService.GetKillProcess("dotnet"), 
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
    }
}
