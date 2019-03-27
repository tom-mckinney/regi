using Newtonsoft.Json;
using Regi.Utilities;
using System;
using System.Collections.Generic;
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

        public IDictionary<string, string> Commands { get; set; } = new Dictionary<string, string>();

        public IList<string> Requires { get; set; } = new List<string>();

        public ProjectOptions Options { get; set; } = new ProjectOptions();

        public int? Port { get; set; }

        public IDictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();

        public bool Serial { get; set; } = false;

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

        [JsonIgnore]
        public AppProcess Process { get; set; }
    }
}
