using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Regi.Services
{
    public interface IDotnetService
    {
        AppProcess RunProject(Project project, CommandOptions options);
        AppProcess TestProject(Project project, CommandOptions options);
        AppProcess RestoreProject(Project project, CommandOptions options);
    }

    public class DotnetService : CLIBase, IDotnetService
    {
        private readonly IConsole _console;
        private readonly string _dotnetPath;

        public DotnetService(IConsole console) : base(console)
        {
            _console = console;

            _dotnetPath = DotNetExe.FullPathOrDefault();

            if (string.IsNullOrWhiteSpace(_dotnetPath))
            {
                throw new FileNotFoundException("Could not find path for .NET Core SDK");
            }
        }

        public AppProcess RestoreProject(Project project, CommandOptions options)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _dotnetPath,
                    Arguments = "restore",
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = options.Verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Install, AppStatus.Running)
            {
                KillOnExit = options.KillProcessesOnExit
            };

            process.ErrorDataReceived += DefaultErrorDataReceived(project.Name, output);
            process.Exited += DefaultExited(output);
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

            return output;
        }

        public AppProcess RunProject(Project project, CommandOptions options)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _dotnetPath,
                    Arguments = "run",
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = options.Verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Start, AppStatus.Running, project.Port)
            {
                KillOnExit = options.KillProcessesOnExit
            };

            process.StartInfo.EnvironmentVariables.Add("END_TO_END_TESTING", true.ToString());
            process.StartInfo.EnvironmentVariables.Add("IN_MEMORY_DATABASE", true.ToString());

            process.StartInfo.CopyEnvironmentVariables(options.VariableList);
            if (project.Port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("ASPNETCORE_URLS", $"http://*:{project.Port}"); // Default .NET Core URL variable
            }

            process.ErrorDataReceived += DefaultErrorDataReceived(project.Name, output);
            process.Exited += DefaultExited(output);
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

        public AppProcess TestProject(Project project, CommandOptions options)
        {
            _console.WriteEmphasizedLine($"Starting tests for project {project.Name} ({project.File.Name})");

            if (string.IsNullOrWhiteSpace(_dotnetPath))
            {
                throw new Exception("Cannot find path to dotnet CLI");
            }

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _dotnetPath,
                    Arguments = "test",
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = options.Verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Test, AppStatus.Running)
            {
                KillOnExit = options.KillProcessesOnExit
            };

            process.StartInfo.CopyEnvironmentVariables(options.VariableList);

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

            process.WaitForExit();

            // Todo: Determine why test doesn't call exit
            output.EndTime = DateTimeOffset.UtcNow;
            if (output.Status == AppStatus.Running)
            {
                output.Status = AppStatus.Success;
            }

            _console.WriteEmphasizedLine($"Finished tests for project {project.Name} ({project.File.Name})");

            return output;
        }
    }
}
