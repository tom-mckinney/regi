using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Regi.Utilities
{
    /// <summary>
    /// Utilities for finding the "npm.cmd" file from the currently running .NET Core application.
    /// </summary>
    public static class NpmExe
    {
        private const string FileName = "npm";

        static NpmExe()
        {
            FullPath = TryFindNpmExePath();
        }

        /// <summary>
        /// The full filepath to the NPM CLI executable.
        /// <para>
        /// May be <c>null</c> if the CLI cannot be found. <seealso cref="FullPathOrDefault" />
        /// </para>
        /// </summary>
        /// <returns>The path or null</returns>
        public static string FullPath { get; }

        /// <summary>
        /// Finds the full filepath to the NPM CLI executable,
        /// or returns a string containing the default name of the NPM muxer ('npm').
        /// <returns>The path or a string named 'npm'</returns>
        /// </summary>
        public static string FullPathOrDefault() => FullPath ?? FileName;

        private static string TryFindNpmExePath()
        {
            var fileName = Environment.ExpandEnvironmentVariables(FileName);

            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName += ".cmd";
            }

            foreach (string pathVar in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
            {
                string path = pathVar.Trim();
                if (!string.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, fileName)))
                    return Path.GetFullPath(path);
            }

            return null;
        }
    }
}
