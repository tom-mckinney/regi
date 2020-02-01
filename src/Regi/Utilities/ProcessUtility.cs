using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Regi.Utilities
{
    public static class ProcessUtility
    {
        public static string AddExtension(string name, string extension = ".exe")
        {
            if (name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return name;
            }

            return name + extension;
        }

        public static void KillProcessWindows(int processId, IFileSystem fileSystem, out string stdout, out string stderr)
        {
            RunProcessAndWaitForExit("taskkill", $"/T /F /PID {processId}", fileSystem, out stdout, out stderr);
        }

        public static void GetAllChildIdsUnix(int parentId, ISet<int> children, IFileSystem fileSystem)
        {
            var exitCode = RunProcessAndWaitForExit(
                "pgrep",
                $"-P {parentId}",
                fileSystem,
                out string stdout,
                out string _);

            if (exitCode == 0 && !string.IsNullOrEmpty(stdout))
            {
                using (var reader = new StringReader(stdout))
                {
                    while (true)
                    {
                        var text = reader.ReadLine();
                        if (text == null)
                        {
                            return;
                        }

                        int id;
                        if (int.TryParse(text, out id))
                        {
                            children.Add(id);
                            // Recursively get the children
                            GetAllChildIdsUnix(id, children, fileSystem);
                        }
                    }
                }
            }
        }

        public static void KillProcessUnix(int processId, IFileSystem fileSystem, out string stdout, out string stderr)
        {
            RunProcessAndWaitForExit(
                "kill",
                $"-TERM {processId}",
                fileSystem,
                out stdout, out stderr);
        }

        public static Process CreateProcess(string fileName, string arguments, IFileSystem fileSystem, string workingDirectory = null)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = fileSystem.GetDirectoryPath(workingDirectory, false) ?? fileSystem.WorkingDirectory,
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };

            return new Process
            {
                StartInfo = startInfo
            };
        }

        public static int RunProcessAndWaitForExit(string fileName, string arguments, IFileSystem fileSystem, out string stdout, out string stderr, int waitToExitMs = 10_000)
        {
            using var process = CreateProcess(fileName, arguments, fileSystem);

            process.Start();

            stdout = null;
            stderr = null;
            if (process.WaitForExit(waitToExitMs))
            {
                stdout = process.StandardOutput.ReadToEnd();
                stderr = process.StandardError.ReadToEnd();
            }
            else
            {
                process.Kill();

                process.WaitForExit((int)TimeSpan.FromSeconds(1).TotalMilliseconds); // Kill is asynchronous so we should still wait a little
            }

            return process.HasExited ? process.ExitCode : -1;
        }

        public static DataReceivedEventHandler WriteOutput(IConsole console, ConsoleLogLevel logLevel = ConsoleLogLevel.Default)
        {
            switch (logLevel)
            {
                case ConsoleLogLevel.Error:
                    return (o, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e?.Data))
                            console.WriteErrorLine(e.Data);
                    };
                case ConsoleLogLevel.Warning:
                    return (o, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e?.Data))
                            console.WriteWarningLine(e.Data);
                    };
                case ConsoleLogLevel.Default:
                default:
                    return (o, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(e?.Data))
                            console.WriteDefaultLine(e.Data);
                    };
            }
        }
    }
}
