using McMaster.Extensions.CommandLineUtils;
using Regi.Constants;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Diagnostics;
using System.Text;

namespace Regi.Services
{
    public interface INodeService : IFrameworkService
    {
    }

    public class NodeService : FrameworkService, INodeService
    {
        public NodeService(IConsole console) : base(console, NpmExe.FullPathOrDefault())
        {
        }

        protected override ProjectOptions FrameworkDefaultOptions => new ProjectOptions();

        protected override void SetEnvironmentVariables(Process process, Project project)
        {
            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("PORT", project.Port.Value.ToString()); // Default NodeJS port variable
            }
        }

        protected override string FormatAdditionalArguments(string args) => $"-- {args}";

        public override AppProcess InstallProject(Project project, CommandOptions options)
        {
            _console.WriteEmphasizedLine($"Starting install for project {project.Name} ({project.File.DirectoryName})");

            AppProcess install = CreateProcess(FrameworkCommands.Node.Install, project, options);

            install.Start();

            install.WaitForExit();

            _console.WriteEmphasizedLine($"Finished installing dependencies for project {project.Name} ({project.File.DirectoryName})");

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
    }
}
