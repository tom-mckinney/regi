using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using Regi.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regi.Frameworks
{
    public interface INode : IFramework
    {
    }

    public class Node : FrameworkBase, INode
    {
        public Node(IConsole console, IPlatformService platformService) : base(console, platformService, NpmExe.FullPathOrDefault())
        {
        }

        public override ProjectFramework Framework => ProjectFramework.Node;
        public override IEnumerable<string> ProcessNames => new[] { "node" };

        public override string BuildCommand => FrameworkCommands.Node.Build;

        protected override IEnumerable<string> FrameworkWarningIndicators => new []
        {
            "npm warn",
            "npm notice",
            "Warning:",
            "clean-webpack-plugin:",
            "[BABEL] Note:",
        };


        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, CommandOptions options)
        {
            if (!string.IsNullOrWhiteSpace(project.Source))
            {
                builder.AppendCliOption($"--registry {project.Source}");
            }

            base.ApplyFrameworkOptions(builder, command, project, options);
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
    }
}
