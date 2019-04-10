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

        public void TryAddSource(CommandOptions options, StartupConfig config)
        {
            if (string.IsNullOrWhiteSpace(Source))
            {
                if (!string.IsNullOrWhiteSpace(options.Source))
                {
                    Source = options.Source;
                }
                else if ((config.Sources.TryGetValue(Framework, out string source) || config.Sources.TryGetValue(ProjectFramework.Any, out source)) && (!string.IsNullOrWhiteSpace(source)))
                {
                    Source = source;
                }
            }
        }

        private FileSystemInfo _fileOrDirectory;
        public FileSystemInfo FileOrDirectory
        {
            get
            {
                if (_fileOrDirectory == null)
                {
                    _fileOrDirectory = FileSystemUtility.GetFileOrDirectory(Path);
                }

                return _fileOrDirectory;
            }
        }

        private string _directoryPath;
        public string DirectoryPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_directoryPath))
                {
                    if (FileOrDirectory is FileInfo file)
                    {
                        _directoryPath = file.DirectoryName;
                    }
                    else if (FileOrDirectory is DirectoryInfo directory)
                    {
                        _directoryPath = directory.FullName;
                    }
                }

                return _directoryPath;
            }
        }

        [JsonIgnore]
        public AppProcess Process { get; set; }
    }
}
