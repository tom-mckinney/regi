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
        AppProcess StartProject(FileInfo projectFile, bool verbose = false, int? port = null);
        AppProcess TestProject(FileInfo project, string pathPattern = null, bool verbose = false);
        AppProcess InstallProject(FileInfo projectFile, bool verbose = false);
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

        public AppProcess InstallProject(FileInfo projectFile, bool verbose = false)
        {
            throw new NotImplementedException();
        }

        public AppProcess StartProject(FileInfo projectFile, bool verbose = false, int? port = null)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _npmPath,
                    Arguments = "start",
                    WorkingDirectory = projectFile.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            if (port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("PORT", port.Value.ToString());
            }

            AppProcess output = new AppProcess(process, AppTask.Run, AppStatus.Running, port);

            process.ErrorDataReceived += DefaultErrorDataReceived(output);
            process.Exited += DefaultExited(output);
            if (verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved();
            }

            process.Start();

            process.BeginErrorReadLine();
            if (verbose)
            {
                process.BeginOutputReadLine();
            }

            return output;
        }

        public AppProcess TestProject(FileInfo projectFile, string pathPattern = null, bool verbose = false)
        {
            _console.WriteEmphasizedLine($"Starting tests for project {projectFile.DirectoryName}");

            if (string.IsNullOrWhiteSpace(_npmPath))
            {
                throw new Exception("Cannot find path to dotnet CLI");
            }

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _npmPath,
                    Arguments = $"test {pathPattern}",
                    WorkingDirectory = projectFile.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Test, AppStatus.Running);

            process.ErrorDataReceived += DefaultOutputDataRecieved();

            if (verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved();
            }

            process.Start();

            process.BeginErrorReadLine();

            if (verbose)
            {
                process.BeginOutputReadLine();
            }

            process.WaitForExit();

            output.Status = process.ExitCode > 0 ? AppStatus.Failure : AppStatus.Success;
            output.End = DateTimeOffset.UtcNow;

            _console.WriteEmphasizedLine($"Finished tests for project {projectFile.DirectoryName}");

            return output;
        }
    }
}
