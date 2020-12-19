using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Frameworks
{
    public interface IDotnet : IFramework
    {
        Task<IAppProcess> ShutdownBuildServer(CommandOptions options, CancellationToken cancellationToken);
    }

    public class Dotnet : FrameworkBase, IDotnet
    {
        private readonly IFileSystem _fileSystem;

        public Dotnet(IFileSystem fileSystem, IConsole console, IPlatformService platformService) : base(console, platformService, DotNetExe.FullPathOrDefault())
        {
            _fileSystem = fileSystem;
        }

        public override ProjectFramework Framework => ProjectFramework.Dotnet;
        public override IEnumerable<string> ProcessNames => new[] { "dotnet" };

        public override string InstallCommand => FrameworkCommands.DotnetCore.Restore;
        public override string StartCommand => FrameworkCommands.DotnetCore.Run;

        protected override CommandDictionary FrameworkOptions => new CommandDictionary
        {
            {
                FrameworkCommands.DotnetCore.Run, new List<string>
                {
                    "--no-launch-profile"
                }
            }
        };

        protected override IEnumerable<string> FrameworkCommandWildcardExclusions => new []
        {
            FrameworkCommands.DotnetCore.Restore,
            FrameworkCommands.DotnetCore.Build
        };

        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, IProject project, CommandOptions options)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (!string.IsNullOrWhiteSpace(project.Source))
            {
                if (command == FrameworkCommands.DotnetCore.Restore || command == FrameworkCommands.DotnetCore.Run || command == FrameworkCommands.DotnetCore.Build || command == FrameworkCommands.DotnetCore.Publish)
                {
                    builder.AppendCliOption($"--source {project.Source}");
                }
            }

            base.ApplyFrameworkOptions(builder, command, project, options);
        }

        protected override void SetEnvironmentVariables(Process process, IProject project)
        {
            base.SetEnvironmentVariables(process, project);

            process.StartInfo.EnvironmentVariables.TryAdd("ASPNETCORE_ENVIRONMENT", "Development");

            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables["ASPNETCORE_URLS"] = $"http://*:{project.Port}"; // Default .NET Core URL variable
            }
        }

        public async Task<IAppProcess> ShutdownBuildServer(CommandOptions options, CancellationToken cancellationToken)
        {
            IAppProcess shutdownBuildServer = CreateProcess(FrameworkCommands.DotnetCore.ShutdownBuildServer, options, _fileSystem);

            shutdownBuildServer.Start();
            await shutdownBuildServer.WaitForExitAsync(cancellationToken);

            return shutdownBuildServer;
        }
    }
}
