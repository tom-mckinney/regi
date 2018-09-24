using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Regi.Services
{
    public interface IDotnetService
    {
        AppProcess RunProject(FileInfo projectFile, bool verbose = false, int? port = null);
        AppProcess TestProject(FileInfo projectFile, bool verbose = false);
        AppProcess RestoreProject(FileInfo projectFile, bool verbose = false);
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

        public AppProcess RestoreProject(FileInfo projectFile, bool verbose = false)
        {
            throw new NotImplementedException();
        }

        public AppProcess RunProject(FileInfo projectFile, bool verbose = false, int? port = null)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _dotnetPath,
                    Arguments = "run",
                    WorkingDirectory = projectFile.DirectoryName,
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

        public AppProcess TestProject(FileInfo projectFile, bool verbose = false)
        {
            _console.WriteEmphasizedLine($"Starting tests for project {projectFile.Name}");

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
                    WorkingDirectory = projectFile.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            AppProcess output = new AppProcess(process, AppTask.Test, AppStatus.Running);

            process.ErrorDataReceived += DefaultErrorDataReceived(output);
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

            // Todo: Determine why test doesn't call exit
            output.End = DateTimeOffset.UtcNow;
            if (output.Status == AppStatus.Running)
            {
                output.Status = AppStatus.Success;
            }

            _console.WriteEmphasizedLine($"Finished tests for project {projectFile.Name}");

            return output;
        }
    }
}
