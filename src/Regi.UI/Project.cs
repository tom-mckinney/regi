using Regi.Models;
using System.Collections.Generic;

namespace Regi
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

        public IList<string> Paths { get; set; }

        public ProjectType Type { get; set; }

        public ProjectFramework Framework { get; set; } = ProjectFramework.Dotnet;

        public IDictionary<string, string> Commands { get; set; } = new Dictionary<string, string>();

        public IList<string> Requires { get; set; } = new List<string>();

        public CommandDictionary Arguments { get; set; } = new CommandDictionary();

        public int? Port { get; set; }

        public IDictionary<string, object> Environment { get; set; } = new Dictionary<string, object>();

        public bool Serial { get; set; } = false;

        public string Source { get; set; }

        public bool RawOutput { get; set; }

        public CommandDictionary<object> Scripts { get; set; }

        public bool Optional { get; set; }

        //public void TryAddSource(RegiOptions options, StartupConfig config)
        //{
        //    if (string.IsNullOrWhiteSpace(Source))
        //    {
        //        if (!string.IsNullOrWhiteSpace(options.Source))
        //        {
        //            Source = options.Source;
        //        }
        //        else if (config.GetSources()?.Count > 0
        //            && (config.GetSources().TryGetValue(Framework, out string source) || config.GetSources().TryGetValue(ProjectFramework.Any, out source))
        //            && !string.IsNullOrWhiteSpace(source))
        //        {
        //            Source = source;
        //        }
        //    }
        //}

        //private IList<string> _appDirectoryPaths;
        //public IList<string> GetAppDirectoryPaths(IFileSystem fileSystem)
        //{
        //    if (_appDirectoryPaths == null)
        //    {
        //        _appDirectoryPaths = new List<string>();

        //        if (Paths?.Count > 0)
        //        {
        //            foreach (var path in Paths)
        //            {
        //                _appDirectoryPaths.Add(fileSystem.GetDirectoryPath(path));
        //            }
        //        }
        //        else if (!string.IsNullOrWhiteSpace(Path))
        //        {
        //            _appDirectoryPaths.Add(fileSystem.GetDirectoryPath(Path));
        //        }
        //    }

        //    return _appDirectoryPaths;
        //}
    }
}
