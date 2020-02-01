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
            if (PathUtility.TryGetPathFile(FileName, out string fileName))
            {
                return fileName;
            }

            return null;
        }
    }
}
