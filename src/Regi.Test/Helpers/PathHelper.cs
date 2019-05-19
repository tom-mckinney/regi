using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Test.Helpers
{
    public static class PathHelper
    {
        private static readonly char Slash = Path.DirectorySeparatorChar;

        internal static string SampleDirectoryPath(string name)
        {
            string path = $"{ProjectRootPath}{Slash}_SampleProjects_{Slash}{name}";

            return path;
        }

        private static string _projectRootPath;
        internal static string ProjectRootPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_projectRootPath))
                {
                    DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
                    while (string.IsNullOrWhiteSpace(_projectRootPath) && currentDirectory != null && currentDirectory.Exists)
                    {
                        var projectFiles = currentDirectory.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
                        if (projectFiles.Length > 0)
                        {
                            _projectRootPath = currentDirectory.FullName;
                        }
                        else
                        {
                            currentDirectory = currentDirectory.Parent;
                        }
                    }
                }

                return _projectRootPath;
            }
        }
    }
}
