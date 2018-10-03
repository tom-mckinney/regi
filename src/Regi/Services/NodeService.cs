using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Regi.Services
{
    public interface INodeService
    {
        AppProcess StartProject(Project project, bool verbose = false, int? port = null);
        AppProcess TestProject(Project project, string pathPattern = null, bool verbose = false);
        AppProcess InstallProject(Project project, bool verbose = false);
    }

    public class NodeService : CLIBase, INodeService
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

        public AppProcess InstallProject(Project project, bool verbose = false)
        {
            _console.WriteEmphasizedLine($"Starting install for project {project.Name} ({project.File.DirectoryName})");

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _npmPath,
                    Arguments = $"install",
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Install, AppStatus.Running);

            process.Exited += DefaultExited(output);
            process.ErrorDataReceived += DefaultOutputDataRecieved(project.Name);
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

            _console.WriteEmphasizedLine($"Finished installing dependencies for project {project.Name} ({project.File.DirectoryName})");

            return output;
            throw new NotImplementedException();
        }

        public AppProcess StartProject(Project project, bool verbose = false, int? port = null)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _npmPath,
                    Arguments = "start",
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            if (port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("PORT", port.Value.ToString());
            }

            AppProcess output = new AppProcess(process, AppTask.Start, AppStatus.Running, port);

            process.Exited += DefaultExited(output);
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

            return output;
        }

        public AppProcess TestProject(Project project, string pathPattern = null, bool verbose = false)
        {
            _console.WriteEmphasizedLine($"Starting tests for project {project.Name} ({project.File.DirectoryName})");

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _npmPath,
                    Arguments = $"test {pathPattern}",
                    WorkingDirectory = project.File.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Test, AppStatus.Running);

            process.ErrorDataReceived += DefaultOutputDataRecieved(project.Name);
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

            output.Status = process.ExitCode > 0 ? AppStatus.Failure : AppStatus.Success;
            output.EndTime = DateTimeOffset.UtcNow;

            _console.WriteEmphasizedLine($"Finished tests for project {project.Name} ({project.File.DirectoryName})");

            return output;
        }
    }
}
