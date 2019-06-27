using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Utilities
{
    public static class ProcessUtility
    {
        private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);

        public static string AddExtension(string name, string extension = ".exe")
        {
            if (name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return name;
            }

            return name + extension;
        }

        public static void KillAllOfType(string processName)
        {
            KillAllOfType(processName, _defaultTimeout);
        }

        public static void KillAllOfType(string processName, TimeSpan timeout)
        {
            if (_isWindows)
            {
                RunProcessAndWaitForExit("taskkill", $"/F /IM {AddExtension(processName)}", out string _, out string _);
            }
            else
            {
                RunProcessAndWaitForExit("killall", $"{processName}", out string _, out string _);
            }
        }

        public static void KillTree(this Process process, int processId)
        {
            KillTree(process, processId, _defaultTimeout, null);
        }

        public static void KillTree(this Process process, int processId, IConsole console)
        {
            KillTree(process, processId, _defaultTimeout, console);
        }

        public static void KillTree(this Process process, int processId, TimeSpan timeout, IConsole console)
        {
            string stdout;
            string stderr;

            if (_isWindows)
            {
                RunProcessAndWaitForExit("taskkill", $"/T /F /PID {processId}", out stdout, out stderr);
                LogOutputs(console, stdout, stderr);
            }
            else
            {
                var children = new HashSet<int>();
                GetAllChildIdsUnix(processId, children);
                foreach (var childId in children)
                {
                    KillProcessUnix(childId, out stdout, out stderr);
                    LogOutputs(console, stdout, stderr);
                }

                KillProcessUnix(processId, out stdout, out stderr);
                LogOutputs(console, stdout, stderr);
            }

            try
            {
                // wait until the process finishes exiting/getting killed. 
                // We don't want to wait forever here because the task is already supposed to be dieing, we just want to give it long enough
                // to try and flush what it can and stop. If it cannot do that in a reasonable time frame then we will just ignore it.
                process?.WaitForExit(timeout.Milliseconds);
            }
            catch (Exception e)
            {
                // TODO: handle this
            }
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

        public static int RunProcessAndWaitForExit(string fileName, string arguments, out string stdout, out string stderr)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);

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

        private static void LogOutputs(IConsole console, string stdout, string stderr)
        {
            if (console != null)
            {
                if (!string.IsNullOrWhiteSpace(stdout))
                    console.WriteDefaultLine(stdout);
                if (!string.IsNullOrWhiteSpace(stderr))
                    console.WriteErrorLine(stderr);
            }
        }
    }
}
