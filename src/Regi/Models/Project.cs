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

        public string Source { get; set; }

        public bool RawOutput { get; set; }

        [JsonIgnore]
        public AppProcess Process { get; set; }

        [JsonIgnore]
        public IList<Project> RequiredProjects { get; set; } = new List<Project>();

        public void TryAddSource(RegiOptions options, StartupConfig config)
        {
            if (string.IsNullOrWhiteSpace(Source))
            {
                if (!string.IsNullOrWhiteSpace(options.Source))
                {
                    Source = options.Source;
                }
                else if (config.Sources?.Count > 0
                    && (config.Sources.TryGetValue(Framework, out string source) || config.Sources.TryGetValue(ProjectFramework.Any, out source))
                    && !string.IsNullOrWhiteSpace(source))
                {
                    Source = source;
                }
            }
        }

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
