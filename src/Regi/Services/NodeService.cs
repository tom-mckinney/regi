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

        public override AppProcess InstallProject(Project project, CommandOptions options)
        {
            _console.WriteEmphasizedLine($"Starting install for project {project.Name} ({project.File.DirectoryName})");
            Process process = DefaultProcess(_npmPath, FrameworkCommands.Node.Install, project, options);

            AppProcess output = new AppProcess(process, AppTask.Install, AppStatus.Running)
            {
                KillOnExit = options.KillProcessesOnExit
            };

            process.Exited += DefaultExited(output);
            process.ErrorDataReceived += DefaultOutputDataRecieved(project.Name);
            if (options.Verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved(project.Name);
            }

            process.Start();

            process.BeginErrorReadLine();
            if (options.Verbose)
            {
                process.BeginOutputReadLine();
            }

            process.WaitForExit();

            _console.WriteEmphasizedLine($"Finished installing dependencies for project {project.Name} ({project.File.DirectoryName})");

            return output;
            throw new NotImplementedException();
        }

        

        public override AppProcess StartProject(Project project, CommandOptions options)
        {
            Process process = DefaultProcess(_npmPath, FrameworkCommands.Node.Start, project, options);

            process.StartInfo.CopyEnvironmentVariables(options.VariableList);
            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("PORT", project.Port.Value.ToString()); // Default NodeJS port variable
            }

            AppProcess output = new AppProcess(process, AppTask.Start, AppStatus.Running, project.Port)
            {
                KillOnExit = options.KillProcessesOnExit
            };

            process.Exited += DefaultExited(output);
            process.ErrorDataReceived += DefaultErrorDataReceived(project.Name, output);
            if (options.Verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved(project.Name);
            }

            process.Start();

            process.BeginErrorReadLine();
            if (options.Verbose)
            {
                process.BeginOutputReadLine();
            }

            return output;
        }

        public override AppProcess TestProject(Project project, CommandOptions options)
        {
            _console.WriteEmphasizedLine($"Starting tests for project {project.Name} ({project.File.DirectoryName})");

            Process process = DefaultProcess(_npmPath, FrameworkCommands.Node.Test, project, options);

            AppProcess output = new AppProcess(process, AppTask.Test, AppStatus.Running)
            {
                KillOnExit = options.KillProcessesOnExit
            };

            process.StartInfo.CopyEnvironmentVariables(options.VariableList);

            process.ErrorDataReceived += DefaultOutputDataRecieved(project.Name);
            if (options.Verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved(project.Name);
            }

            process.Start();

            process.BeginErrorReadLine();
            if (options.Verbose)
            {
                process.BeginOutputReadLine();
            }

            process.WaitForExit();

            output.Status = process.ExitCode > 0 ? AppStatus.Failure : AppStatus.Success;
            output.EndTime = DateTimeOffset.UtcNow;

            _console.WriteEmphasizedLine($"Finished tests for project {project.Name} ({project.File.DirectoryName})");

            return output;
        }

        protected override string FormatAdditionalArguments(string args)
        {
            return $"-- {args}";
        }
    }
}
