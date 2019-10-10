using System;
using System.IO;

namespace Regi.Utilities
{
    public static class DirectoryUtility
    {
        private static string _workingDirectory;
        public static string WorkingDirectory
        {
            get
            {
                if (_workingDirectory == null)
                {
                    _workingDirectory = Directory.GetCurrentDirectory();
                }

                return _workingDirectory;
            }
        }

        public static void SetWorkingDirectory(string path)
        {
            _workingDirectory = path;
        }

        public static string GetDirectoryPath(string path, bool throwIfNotFound = true, string targetObj = "project")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (throwIfNotFound)
                {
                    throw new ArgumentException("Path cannot be null", nameof(path));
                }
                else
                {
                    return null;
                }
            }

            string absolutePath = Path.GetFullPath(path, WorkingDirectory);

            if (Directory.Exists(absolutePath))
            {
                return absolutePath;
            }
            else if (File.Exists(absolutePath))
            {
                return Path.GetDirectoryName(absolutePath);
            }
            else if (throwIfNotFound)
            {
                throw new DirectoryNotFoundException($"Could not find {targetObj}, {absolutePath}");
            }

            return null;
        }

        public static string GetDirectoryShortName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Must specify a valid directory path", nameof(path));

            return Path.GetFileName(path);
        }

        public static void ResetTargetDirectory()
        {
            _workingDirectory = null;
        }
    }
}
