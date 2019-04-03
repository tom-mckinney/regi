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

        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, CommandOptions options)
        {
            if (!string.IsNullOrWhiteSpace(project.Source))
            {
                FrameworkOptions.AddOptions(FrameworkCommands.Any, $"--registry {project.Source}");
            }

            base.ApplyFrameworkOptions(builder, command, project, options);
        }

        protected override void SetEnvironmentVariables(Process process, Project project)
        {
            base.SetEnvironmentVariables(process, project);

            process.StartInfo.EnvironmentVariables.Add("CI", bool.TrueString);

            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("PORT", project.Port.Value.ToString()); // Default NodeJS port variable
            }
        }

        protected override string FormatAdditionalArguments(string args) => $"-- {args}";

        public override AppProcess InstallProject(Project project, CommandOptions options)
        {
            AppProcess install = CreateProcess(FrameworkCommands.Node.Install, project, options);

            install.Start();

            install.WaitForExit();

            return install;
        }

        public override AppProcess StartProject(Project project, CommandOptions options)
        {
            AppProcess start = CreateProcess(FrameworkCommands.Node.Start, project, options);

            start.Start();

            return start;
        }

        public override AppProcess TestProject(Project project, CommandOptions options)
        {
            AppProcess test = CreateProcess(FrameworkCommands.Node.Test, project, options);

            test.Start();

            test.WaitForExit();

            test.Status = test.Process.ExitCode > 0 ? AppStatus.Failure : AppStatus.Success;
            test.EndTime = DateTimeOffset.UtcNow;

            return test;
        }

        public override DataReceivedEventHandler HandleErrorDataReceived(string name, AppProcess output) => new DataReceivedEventHandler((o, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data) && !e.Data.StartsWith("npm warn", StringComparison.InvariantCultureIgnoreCase))
            {
                output.Status = AppStatus.Failure;
                _console.WriteErrorLine(name + ": " + e.Data);
            }
        });

        public override AppProcess KillProcesses(CommandOptions options)
        {
            AppProcess process = new AppProcess(_platformService.GetKillProcess("node"),
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
