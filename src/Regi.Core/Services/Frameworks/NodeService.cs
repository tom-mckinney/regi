using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Services.Frameworks
{
    public interface INodeService : IFrameworkService
    {
    }

    public class NodeService : FrameworkService, INodeService
    {
        public NodeService(IConsole console, IPlatformService platformService) : base(console, platformService, NpmExe.FullPathOrDefault())
        {
        }

        protected override CommandDictionary FrameworkOptions { get; } = new CommandDictionary();

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

        protected override string FormatAdditionalArguments(IEnumerable<string> args) => $"-- {string.Join(' ', args)}";

        public override async Task<AppProcess> InstallProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            AppProcess install = CreateProcess(FrameworkCommands.Node.Install, project, appDirectoryPath, options);

            install.Start();

            await install.WaitForExitAsync(cancellationToken);

            return install;
        }

        public override Task<AppProcess> StartProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                AppProcess start = CreateProcess(FrameworkCommands.Node.Start, project, appDirectoryPath, options);

                start.Start();

                return start;
            }, cancellationToken);
        }

        public override async Task<AppProcess> TestProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            AppProcess test = CreateProcess(FrameworkCommands.Node.Test, project, appDirectoryPath, options);

            test.Start();

            await test.WaitForExitAsync(cancellationToken);

            return test;
        }

        public override Task<AppProcess> BuildProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            _console.WriteWarningLine($"Did not build {project.Name}. No implementation for {nameof(BuildProject)} in {nameof(NodeService)}.");

            return Task.FromResult(new AppProcess(null, AppTask.Build, AppStatus.Unknown));
        }

        public override async Task<AppProcess> KillProcesses(RegiOptions options, CancellationToken cancellationToken)
        {
            AppProcess process = new AppProcess(_platformService.GetKillProcess("node", options),
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
    }
}
