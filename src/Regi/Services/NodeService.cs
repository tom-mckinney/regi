using McMaster.Extensions.CommandLineUtils;
using Regi.Constants;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Regi.Services
{
    public interface INodeService : IFrameworkService
    {
    }

    public class NodeService : FrameworkService, INodeService
    {
        private readonly IConsole _console;
        private readonly string _npmPath;

        public NodeService(IConsole console) : base(console)
        {
            _console = console;

            _npmPath = NpmExe.FullPathOrDefault();

            if (string.IsNullOrWhiteSpace(_npmPath))
            {
                throw new FileNotFoundException("Could not find path for NPM CLI");
            }
        }

        protected override ProjectOptions FrameworkDefaultOptions => new ProjectOptions();

        protected override void SetEnvironmentVariables(Process process, Project project)
        {
            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("PORT", project.Port.Value.ToString()); // Default NodeJS port variable
            }
        }

        protected override string FormatAdditionalArguments(string args)
        {
            return $"-- {args}";
        }

        public override AppProcess InstallProject(Project project, CommandOptions options)
        {
            _console.WriteEmphasizedLine($"Starting install for project {project.Name} ({project.File.DirectoryName})");

            AppProcess install = CreateProcess(_npmPath, FrameworkCommands.Node.Install, project, options);

            //process.ErrorDataReceived += DefaultOutputDataRecieved(project.Name); // TODO: do we need this?

            install.Start();

            install.WaitForExit();

            _console.WriteEmphasizedLine($"Finished installing dependencies for project {project.Name} ({project.File.DirectoryName})");

            return install;
        }

        public override AppProcess StartProject(Project project, CommandOptions options)
        {
            AppProcess start = CreateProcess(_npmPath, FrameworkCommands.Node.Start, project, options);

            start.Start();

            return start;
        }

        public override AppProcess TestProject(Project project, CommandOptions options)
        {
            _console.WriteEmphasizedLine($"Starting tests for project {project.Name} ({project.File.DirectoryName})");

            AppProcess test = CreateProcess(_npmPath, FrameworkCommands.Node.Test, project, options);

            //process.StartInfo.CopyEnvironmentVariables(options.VariableList);

            test.Start();

            test.WaitForExit();

            test.Status = test.Process.ExitCode > 0 ? AppStatus.Failure : AppStatus.Success;
            test.EndTime = DateTimeOffset.UtcNow;

            _console.WriteEmphasizedLine($"Finished tests for project {project.Name} ({project.File.DirectoryName})");

            return test;
        }
    }
}
