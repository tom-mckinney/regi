using McMaster.Extensions.CommandLineUtils;
using Regi.Constants;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regi.Services
{
    public interface INodeService : IFrameworkService
    {
    }

    public class NodeService : FrameworkService, INodeService
    {
        public NodeService(IConsole console, IPlatformService platformService) : base(console, platformService, NpmExe.FullPathOrDefault())
        {
        }

        protected override ProjectOptions FrameworkOptions { get; } = new ProjectOptions();

        protected override IList<string> FrameworkWarningIndicators => new List<string>
        {
            "npm warn",
            "npm notice",
            "Warning:",
            "clean-webpack-plugin:",
            "[BABEL] Note:",
        };

        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, RegiOptions options)
        {
            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(project.Source))
                {
                    builder.AppendCliOption($"--registry {project.Source}");
                }

                base.ApplyFrameworkOptions(builder, command, project, options);
            }
        }

        protected override void SetEnvironmentVariables(Process process, Project project)
        {
            base.SetEnvironmentVariables(process, project);

            process.StartInfo.EnvironmentVariables.TryAdd("CI", bool.TrueString);

            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables["PORT"] = project.Port.Value.ToString(); // Default NodeJS port variable
            }
        }

        protected override string FormatAdditionalArguments(string[] args) => $"-- {string.Join(' ', args)}";

        public override AppProcess InstallProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            AppProcess install = CreateProcess(FrameworkCommands.Node.Install, project, appDirectoryPath, options);

            install.Start();

            install.WaitForExit();

            return install;
        }

        public override AppProcess StartProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            AppProcess start = CreateProcess(FrameworkCommands.Node.Start, project, appDirectoryPath, options);

            start.Start();

            return start;
        }

        public override AppProcess TestProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            AppProcess test = CreateProcess(FrameworkCommands.Node.Test, project, appDirectoryPath, options);

            test.Start();

            test.WaitForExit();

            return test;
        }

        public override AppProcess BuildProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            _console.WriteWarningLine($"Did not build {project.Name}. No implementation for {nameof(BuildProject)} in {nameof(NodeService)}.");

            return new AppProcess(null, AppTask.Build, AppStatus.Unknown);
        }

        public override AppProcess KillProcesses(RegiOptions options)
        {
            AppProcess process = new AppProcess(_platformService.GetKillProcess("node", options),
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
