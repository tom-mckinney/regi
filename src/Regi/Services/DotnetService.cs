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
        DotnetProcess TestProject(FileInfo projectFile, bool verbose = false);
        DotnetProcess RunProject(FileInfo projectFile, bool verbose = false);
    }

    public class DotnetService : IDotnetService
    {
        private readonly IConsole _console;
        private readonly string _dotnetPath;

        public DotnetService(IConsole console)
        {
            _console = console;

            _dotnetPath = DotNetExe.FullPathOrDefault();

            if (string.IsNullOrWhiteSpace(_dotnetPath))
            {
                throw new FileNotFoundException("Could not find path for .NET Core SDK");
            }
        }

        public DotnetProcess RunProject(FileInfo projectFile, bool verbose = false)
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
            process.StartInfo.EnvironmentVariables.Add("ASPNETCORE_URLS", $"http://*:{5000}");

            DotnetProcess output = new DotnetProcess(process, DotnetTask.Run, DotnetStatus.Running);

            process.ErrorDataReceived += (o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    output.Status = DotnetStatus.Failure;
                    _console.WriteErrorLine(e.Data);
                }
            };

            if (verbose)
            {
                process.OutputDataReceived += (o, e) =>
                {
                    _console.WriteLine(e.Data);
                };
            }

            process.Exited += (o, e) =>
            {
                if (output.Status == DotnetStatus.Running)
                {
                    output.Status = DotnetStatus.Success;
                }
            };

            process.Start();

            process.BeginErrorReadLine();
            if (verbose)
            {
                process.BeginOutputReadLine();
            }            

            return output;
        }

        public DotnetProcess TestProject(FileInfo projectFile, bool verbose = false)
        {
            _console.ForegroundColor = ConsoleColor.Cyan;
            _console.WriteLine($"Starting tests for project {projectFile.Name}");
            _console.ResetColor();

            DotnetStatus status = DotnetStatus.Success;

            if (string.IsNullOrWhiteSpace(_dotnetPath))
            {
                throw new Exception("Cannot find path to dotnet CLI");
            }

            Process unitTest = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _dotnetPath,
                    Arguments = "test",
                    WorkingDirectory = projectFile.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                }
            };

            unitTest.ErrorDataReceived += (o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    status = DotnetStatus.Failure;

                    _console.BackgroundColor = ConsoleColor.DarkRed;
                    _console.ForegroundColor = ConsoleColor.White;

                    _console.WriteLine(e.Data);

                    _console.ResetColor();
                }
            };

            if (verbose)
            {
                unitTest.OutputDataReceived += (o, e) =>
                {
                    _console.WriteLine(e.Data);
                };
            }

            unitTest.Start();

            unitTest.BeginErrorReadLine();

            if (verbose)
            {
                unitTest.BeginOutputReadLine();
            }

            unitTest.WaitForExit();

            _console.ForegroundColor = ConsoleColor.Cyan;
            _console.WriteLine($"Finished tests for project {projectFile.Name}");
            _console.ResetColor();

            return new DotnetProcess(unitTest, DotnetTask.Test, status)
            {
                End = DateTimeOffset.UtcNow
            };
        }
    }
}
