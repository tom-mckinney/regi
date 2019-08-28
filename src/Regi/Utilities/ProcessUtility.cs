using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
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

        public static void GetAllChildIdsUnix(int parentId, ISet<int> children)
        {
            var exitCode = RunProcessAndWaitForExit(
                "pgrep",
                $"-P {parentId}",
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
                            GetAllChildIdsUnix(id, children);
                        }
                    }
                }
            }
        }

        public static void KillProcessUnix(int processId, out string stdout, out string stderr)
        {
            RunProcessAndWaitForExit(
                "kill",
                $"-TERM {processId}",
                out stdout, out stderr);
        }

        public static Process CreateProcess(string fileName, string arguments, string workingDirectory = null)
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = DirectoryUtility.GetDirectoryPath(workingDirectory, false) ?? DirectoryUtility.TargetDirectoryPath,
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

        public static int RunProcessAndWaitForExit(string fileName, string arguments, out string stdout, out string stderr)
        {
            using (var process = CreateProcess(fileName, arguments))
            {
                process.Start();

                stdout = null;
                stderr = null;
                if (process.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds))
                {
                    stdout = process.StandardOutput.ReadToEnd();
                    stderr = process.StandardError.ReadToEnd();
                }
                else
                {
                    process.Kill();

                    // Kill is asynchronous so we should still wait a little
                    //
                    process.WaitForExit((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
                }

                return process.HasExited ? process.ExitCode : -1;
            }
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
