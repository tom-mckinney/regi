using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Test.Helpers
{
    public static class PathHelper
    {
        internal static string GetSampleProjectPath(string name) => Path.Combine(SampleProjectsRootPath, name);

        internal static string SampleProjectsRootPath => Path.Combine(RegiTestRootPath, "_SampleProjects_");

        private static string _regiTestRootPath;
        internal static string RegiTestRootPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_regiTestRootPath))
                {
                    DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
                    while (string.IsNullOrWhiteSpace(_regiTestRootPath) && currentDirectory != null && currentDirectory.Exists)
                    {
                        var projectFiles = currentDirectory.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
                        if (projectFiles.Length > 0)
                        {
                            _regiTestRootPath = currentDirectory.FullName;
                        }
                        else
                        {
                            currentDirectory = currentDirectory.Parent;
                        }
                    }
                }

                return _regiTestRootPath;
            }
        }
    }
}
