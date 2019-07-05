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

        public static string GetDirectoryPath(string path)
        {
            string absolutePath = Path.GetFullPath(path, TargetDirectoryPath);

            if (Directory.Exists(absolutePath))
            {
                return absolutePath;
            }
            else if (File.Exists(absolutePath))
            {
                return Path.GetDirectoryName(absolutePath);
            }
            else
            {
                throw new DirectoryNotFoundException($"Could not find project, {absolutePath}");
            }
        }
    }
}
