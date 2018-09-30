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
    }
}
