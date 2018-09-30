using Regi.Utilities;
using System.IO;

namespace Regi.Models
{
    public class Project
    {
        public Project() { }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; set; } = "Unnamed Project";

        public string Path { get; set; }

        public ProjectType Type { get; set; }

        public ProjectFramework Framework { get; set; } = ProjectFramework.Dotnet;

        public int? Port { get; set; }

        private FileInfo _file;
        public FileInfo File
        {
            get
            {
                if (_file == null)
                {
                    string absolutePath = System.IO.Path.GetFullPath(Path, DirectoryUtility.TargetDirectoryPath);
                    _file = new FileInfo(absolutePath);

                    if (!_file.Exists)
                    {
                        throw new FileNotFoundException($"Could not find project, {_file.FullName}");
                    }
                }

                return _file;
            }
        }
    }
}
