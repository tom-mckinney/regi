using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Utilities
{
    public static class PathUtility
    {
        public static bool TryGetPathFile(string fileName, out string pathToFile)
        {
            return TryGetPathFile(fileName, out pathToFile, RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        }

        public static bool TryGetPathFile(string fileName, out string pathToFile, bool isWindows)
        {
            fileName = Environment.ExpandEnvironmentVariables(fileName);

            if (File.Exists(fileName))
            {
                pathToFile = Path.GetFullPath(fileName);
                return true;
            }

            if (isWindows && !fileName.EndsWith(".cmd"))
            {
                fileName += ".cmd";
            }

            foreach (string pathVar in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
            {
                string path = pathVar.Trim();
                if (!string.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, fileName)))
                {
                    pathToFile = Path.GetFullPath(path);
                    return true;
                }
            }

            pathToFile = null;
            return false;
        }

        public static string GetFileNameFromScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                throw new ArgumentException("Script cannot be null, empty, or white space");
            }

            int separatorIndex = script.IndexOf(' ');
            return script.Substring(0, separatorIndex != -1 ? separatorIndex : script.Length);
        }
    }
}
