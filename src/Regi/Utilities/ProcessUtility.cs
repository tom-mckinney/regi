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
                RunProcessAndWaitForExit("killall", $"-KILL {processName}", timeout);
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
                RunProcessAndWaitForExit(
                    "taskkill",
                    $"/T /F /PID {processId}",
                    timeout);
            }
            else
            {
                var children = new HashSet<int>();
                GetAllChildIdsUnix(processId, children, timeout);
                foreach (var childId in children)
                {
                    KillProcessUnix(childId, timeout);
                }
                KillProcessUnix(processId, timeout);
            }
        }

        private static void GetAllChildIdsUnix(int parentId, ISet<int> children, TimeSpan timeout)
        {
            string stdout;
            var exitCode = RunProcessAndWaitForExit(
                "pgrep",
                $"-P {parentId}",
                timeout);

            //if (exitCode == 0)
            //{
            //    using (var reader = new StringReader(stdout))
            //    {
            //        while (true)
            //        {
            //            var text = reader.ReadLine();
            //            if (text == null)
            //            {
            //                return;
            //            }

            //            if (int.TryParse(text, out int id))
            //            {
            //                children.Add(id);
            //                // Recursively get the children
            //                GetAllChildIdsUnix(id, children, timeout);
            //            }
            //        }
            //    }
            //}
        }

        private static void KillProcessUnix(int processId, TimeSpan timeout)
        {
            RunProcessAndWaitForExit(
                "kill",
                $"-TERM {processId}",
                timeout);
        }

        private static int RunProcessAndWaitForExit(string fileName, string arguments, TimeSpan timeout)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = true,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);

            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                process.Kill();
            }

            return process.ExitCode;
        }

        private static string AddExtension(string name, string extension = ".exe")
        {
            if (name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
            {
                return name;
            }

            return name + extension;
        }
    }
}
