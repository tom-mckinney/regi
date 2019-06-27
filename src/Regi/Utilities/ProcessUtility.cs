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

        public static void KillAllOfType(string processName)
        {
            KillAllOfType(processName, _defaultTimeout);
        }

        public static void KillAllOfType(string processName, TimeSpan timeout)
        {
            if (_isWindows)
            {
                RunProcessAndWaitForExit("taskkill", $"/F /IM {AddExtension(processName)}", timeout);
            }
            else
            {
                RunProcessAndWaitForExit("killall", $"{processName}", timeout);
            }
        }

        public static void KillTree(int processId)
        {
            KillTree(processId, _defaultTimeout);
        }

        public static void KillTree(int processId, TimeSpan timeout)
        {
            if (_isWindows)
            {
                RunProcessAndWaitForExit("taskkill", $"/T /F /PID {processId}", timeout);
            }
            else
            {
                RunProcessAndWaitForExit("kill", $"-TERM {processId}", timeout);
            }
        }

        private static int RunProcessAndWaitForExit(string fileName, string arguments, TimeSpan timeout)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            process.Start();

            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                process.Kill();
            }

            return process.ExitCode;
        }

        public static string AddExtension(string name, string extension = ".exe")
        {
            if (name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return name;
            }

            return name + extension;
        }
    }
}
