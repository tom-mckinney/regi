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
        public DotnetService(IConsole console) : base(console, DotNetExe.FullPathOrDefault())
        {
        }

        protected override ProjectOptions FrameworkOptions { get; } = new ProjectOptions
        {
            { FrameworkCommands.Dotnet.Run, new List<string> { "--no-launch-profile" } }
        };

        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, CommandOptions options)
        {
            if (!string.IsNullOrWhiteSpace(project.Source))
            {
                FrameworkOptions.AddOptions(FrameworkCommands.Any, $"--source {project.Source}");
            }

            base.ApplyFrameworkOptions(builder, command, project, options);
        }

        protected override void SetEnvironmentVariables(Process process, Project project)
        {
            base.SetEnvironmentVariables(process, project);

            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("ASPNETCORE_URLS", $"http://*:{project.Port}"); // Default .NET Core URL variable
            }
        }

        public override AppProcess InstallProject(Project project, CommandOptions options)
        {
            AppProcess install = CreateProcess(FrameworkCommands.Dotnet.Restore, project, options);

            install.Start();

            install.WaitForExit();

            return install;
        }

        public override AppProcess StartProject(Project project, CommandOptions options)
        {
            AppProcess start = CreateProcess(FrameworkCommands.Dotnet.Run, project, options);

            start.Start();

            return start;
        }

        public override AppProcess TestProject(Project project, CommandOptions options)
        {
            AppProcess test = CreateProcess(FrameworkCommands.Dotnet.Test, project, options);

            test.Start();

            test.WaitForExit();

            // Todo: Determine why test doesn't call exit
            test.EndTime = DateTimeOffset.UtcNow;
            if (test.Status == AppStatus.Running)
            {
                test.Status = AppStatus.Success;
            }

            return test;
        }
    }
}
