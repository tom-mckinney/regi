using System;
using System.IO;

namespace Regi.Utilities
{
    public static class DirectoryUtility
    {
        private static string _targetDirectoryPath;
        public static string TargetDirectoryPath
        {
            get
            {
                if (_targetDirectoryPath == null)
                {
                    _targetDirectoryPath = Directory.GetCurrentDirectory();
                }

                return _targetDirectoryPath;
            }
        }

        public static void SetTargetDirectory(string path)
        {
            _targetDirectoryPath = path;
        }

        public static string GetDirectoryPath(string path, bool throwIfNotFound = true)
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

            string absolutePath = Path.GetFullPath(path, TargetDirectoryPath);

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
                throw new DirectoryNotFoundException($"Could not find project, {absolutePath}");
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
            _targetDirectoryPath = null;
        }
    }
}
