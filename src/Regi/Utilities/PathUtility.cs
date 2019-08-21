using Regi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Utilities
{
    public static class PathUtility
    {
        private static readonly string[] WindowsExtensionsLookup = new string[] { ".exe", ".cmd" };

        public static bool TryGetPathFile(string fileName, out string pathToFile)
        {
            return TryGetPathFile(fileName, new RuntimeInfo(), out pathToFile);
        }

        public static bool TryGetPathFile(string fileName, IRuntimeInfo runtimeInfo, out string pathToFile)
        {
            fileName = Environment.ExpandEnvironmentVariables(fileName);

            if (File.Exists(fileName))
            {
                pathToFile = Path.GetFullPath(fileName);
                return true;
            }

            char pathSeparator = runtimeInfo.IsWindows ? ';' : ':';
            string[] pathVariables = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(pathSeparator);
            foreach (var pathVar in pathVariables)
            {
                string path = pathVar.Trim();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    if (runtimeInfo.IsWindows)
                    {
                        string pathWithExtension;
                        foreach (var extension in WindowsExtensionsLookup)
                        {
                            if (File.Exists(pathWithExtension = Path.Combine(path, fileName + extension)))
                            {
                                pathToFile = Path.GetFullPath(pathWithExtension);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(path = Path.Combine(path, fileName)))
                        {
                            pathToFile = Path.GetFullPath(path);
                            return true;
                        }
                    }
                }
            }

            pathToFile = null;
            return false;
        }

        public static string GetFileNameFromCommand(string script)
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
