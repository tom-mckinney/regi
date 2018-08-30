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
        private readonly StringBuilder _stringBuilder;
        private IConsole _console;

        public DotnetService(IConsole console)
        {
            _stringBuilder = new StringBuilder();
            _console = console;
        }

        public DotnetProcess RunProject(FileInfo projectFile, bool verbose = false)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = DotNetExe.FullPath,
                    Arguments = "run",
                    WorkingDirectory = projectFile.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

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
            DotnetStatus status = DotnetStatus.Success;

            Process unitTest = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = DotNetExe.FullPath,
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

            return new DotnetProcess(unitTest, DotnetTask.Test, status)
            {
                End = DateTimeOffset.UtcNow
            };
        }
    }
}
