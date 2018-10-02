﻿using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace Regi.Services
{
    public interface IDotnetService
    {
        AppProcess RunProject(Project project, bool verbose = false, int? port = null);
        AppProcess TestProject(Project project, bool verbose = false);
        AppProcess RestoreProject(Project project, bool verbose = false);
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

        public AppProcess RestoreProject(Project project, bool verbose = false)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _dotnetPath,
                    Arguments = "restore",
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Install, AppStatus.Running);

            process.ErrorDataReceived += DefaultErrorDataReceived(project.Name, output);
            process.Exited += DefaultExited(output);
            if (verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved(project.Name);
            }

            process.Start();

            process.BeginErrorReadLine();
            if (verbose)
            {
                process.BeginOutputReadLine();
            }

            process.WaitForExit();

            return output;
        }

        public AppProcess RunProject(Project project, bool verbose = false, int? port = null)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _dotnetPath,
                    Arguments = "run",
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            process.StartInfo.EnvironmentVariables.Add("END_TO_END_TESTING", true.ToString());
            process.StartInfo.EnvironmentVariables.Add("IN_MEMORY_DATABASE", true.ToString());

            if (port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("ASPNETCORE_URLS", $"http://*:{port}");
            }

            AppProcess output = new AppProcess(process, AppTask.Start, AppStatus.Running, port);

            process.ErrorDataReceived += DefaultErrorDataReceived(project.Name, output);
            process.Exited += DefaultExited(output);
            if (verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved(project.Name);
            }

            process.Start();

            process.BeginErrorReadLine();
            if (verbose)
            {
                process.BeginOutputReadLine();
            }

            return output;
        }

        public AppProcess TestProject(Project project, bool verbose = false)
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
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Test, AppStatus.Running);

            process.ErrorDataReceived += DefaultErrorDataReceived(project.Name, output);
            if (verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved(project.Name);
            }

            process.Start();

            process.BeginErrorReadLine();

            if (verbose)
            {
                process.BeginOutputReadLine();
            }

            process.WaitForExit();

            // Todo: Determine why test doesn't call exit
            output.End = DateTimeOffset.UtcNow;
            if (output.Status == AppStatus.Running)
            {
                output.Status = AppStatus.Success;
            }

            _console.WriteEmphasizedLine($"Finished tests for project {project.Name} ({project.File.Name})");

            return output;
        }
    }
}
