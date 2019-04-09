using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Regi.Utilities
{
    public static class FileSystemUtility
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

        public static IList<FileSystemInfo> GetFilesOrDirectories(IEnumerable<string> paths)
        {
            return paths.Select(p => GetFileOrDirectory(p)).ToList();
        }

        public static FileSystemInfo GetFileOrDirectory(string path)
        {
            string absolutePath = Path.GetFullPath(path, TargetDirectoryPath);

            FileSystemInfo fileOrDirectory = null;

            if (File.Exists(absolutePath))
            {
                fileOrDirectory = new FileInfo(absolutePath);
            }
            else if (Directory.Exists(absolutePath))
            {
                fileOrDirectory = new DirectoryInfo(absolutePath);
            }

            if (fileOrDirectory == null || !fileOrDirectory.Exists)
            {
                if (Path.HasExtension(absolutePath))
                    throw new FileNotFoundException($"Could not find project file {absolutePath}");
                else
                    throw new DirectoryNotFoundException($"Could not find project directory {absolutePath}");
            }

            return fileOrDirectory;
        }
    }
}
