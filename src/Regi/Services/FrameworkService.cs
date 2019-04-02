using McMaster.Extensions.CommandLineUtils;
using Regi.Constants;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Services
{
    public interface IFrameworkService
    {
        AppProcess InstallProject(Project project, CommandOptions options);
        AppProcess StartProject(Project project, CommandOptions options);
        AppProcess TestProject(Project project, CommandOptions options);
    }

    public abstract class FrameworkService : IFrameworkService
    {
        protected readonly IConsole _console;
        protected readonly string _frameworkExePath;
        protected readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public FrameworkService(IConsole console, string frameworkExePath)
        {
            _console = console;

            if (string.IsNullOrWhiteSpace(frameworkExePath))
            {
                throw new ArgumentException("Cannot have a null or empty framework executable path.", nameof(frameworkExePath));
            }

            _frameworkExePath = frameworkExePath;
        }

        public abstract AppProcess InstallProject(Project project, CommandOptions options);
        public abstract AppProcess StartProject(Project project, CommandOptions options);
        public abstract AppProcess TestProject(Project project, CommandOptions options);

        protected virtual void SetEnvironmentVariables(Process process, Project project)
        {
            process.StartInfo.EnvironmentVariables.Add("END_TO_END_TESTING", bool.TrueString);
            process.StartInfo.EnvironmentVariables.Add("IN_MEMORY_DATABASE", bool.TrueString);
            process.StartInfo.EnvironmentVariables.Add("DONT_STUB_PLAID", bool.FalseString);
        }

        protected abstract ProjectOptions FrameworkOptions { get; }

        protected virtual void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, CommandOptions options)
        {
            if (FrameworkOptions != null && FrameworkOptions.Any())
            {
                if (FrameworkOptions.TryGetValue(command, out IList<string> defaultOptions))
                {
                    builder.Append(' ').AppendJoin(' ', defaultOptions);
                }

                if (FrameworkOptions.TryGetValue(FrameworkCommands.Any, out IList<string> anyCommandOptions))
                {
                    builder.Append(' ').AppendJoin(' ', anyCommandOptions);
                }
            }
        }

        protected virtual string FormatAdditionalArguments(string args) => args;

        public virtual AppProcess CreateProcess(string command, Project project, CommandOptions options)
        {
            string args = BuildCommand(command, project, options);

            if (options.Verbose)
                _console.WriteEmphasizedLine($"Executing: {_frameworkExePath} {args}");

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _frameworkExePath,
                    Arguments = args,
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = _isWindows ? options.Verbose : true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, FrameworkCommands.GetAppTask(command), AppStatus.Running, project.Port)
            {
                KillOnExit = options.KillProcessesOnExit,
                Verbose = options.Verbose,
                OnDispose = (processId) => HandleDispose(project, processId)
            };

            if (project.RawOutput)
            {
                output.RawOutput = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                return output;
            }

            process.StartInfo.CopyEnvironmentVariables(options.VariableList);
            SetEnvironmentVariables(process, project);

            process.ErrorDataReceived += HandleErrorDataReceived(project.Name, output);
            process.Exited += HandleExited(output);

            if (options.Verbose)
            {
                process.OutputDataReceived += HandleOutputDataRecieved(project.Name);
            }

            return output;
        }

        public virtual string BuildCommand(string command, Project project, CommandOptions options)
        {
            if (project?.Commands != null && project.Commands.TryGetValue(command, out string customCommand))
            {
                command = customCommand;
            }

            StringBuilder builder = new StringBuilder();

            builder.Append(command);

            if (project?.Options?.Count > 0)
            {
                foreach (var commandOption in project.Options)
                {
                    if (commandOption.Key == "*" || commandOption.Key == command)
                    {

                        builder.Append(' ').AppendJoin(' ', commandOption.Value);
                    }
                }
            }

            ApplyFrameworkOptions(builder, command, project, options);

            if (!string.IsNullOrWhiteSpace(options?.Arguments))
            {
                builder.Append(' ').Append(FormatAdditionalArguments(options.Arguments));
            }

            return builder.ToString();
        }

        public virtual DataReceivedEventHandler HandleOutputDataRecieved(string name) => new DataReceivedEventHandler((o, e) =>
        {
            _console.WriteLine(name + ": " + e.Data);
        });

        public virtual DataReceivedEventHandler HandleErrorDataReceived(string name, AppProcess output) => new DataReceivedEventHandler((o, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                output.Status = AppStatus.Failure;
                _console.WriteErrorLine(name + ": " + e.Data);
            }
        });

        public virtual EventHandler HandleExited(AppProcess output) => new EventHandler((o, e) =>
        {
            output.EndTime = DateTimeOffset.UtcNow;

            if (output.Status == AppStatus.Running)
            {
                output.Status = AppStatus.Success;
            }
        });

        protected virtual void HandleDispose(Project project, int processId)
        {
            _console.WriteEmphasizedLine($"Disposing process for project {project.Name} ({processId})");
        }
    }
}
