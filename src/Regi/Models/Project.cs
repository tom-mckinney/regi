using Newtonsoft.Json;
using Regi.Utilities;
using System;
using System.Collections.Concurrent;
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

        public IList<string> Paths { get; set; }

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
        public ConcurrentBag<AppProcess> Processes { get; set; } = new ConcurrentBag<AppProcess>();

        [JsonIgnore]
        public ConcurrentBag<Project> RequiredProjects { get; set; } = new ConcurrentBag<Project>();

        [JsonIgnore]
        public AppStatus OutputStatus
        {
            get
            {
                int successCount = 0;
                int failCount = 0;
                int runningCount = 0;

                foreach (var process in Processes)
                {
                    switch (process.Status)
                    {
                        case AppStatus.Success:
                            successCount++;
                            break;
                        case AppStatus.Failure:
                            failCount++;
                            break;
                        case AppStatus.Running:
                            runningCount++;
                            break;
                        default:
                            break;
                    }
                }

                if (successCount == Processes.Count)
                    return AppStatus.Success;
                else if (failCount > 0)
                    return AppStatus.Failure;
                else if (runningCount > 0)
                    return AppStatus.Running;
                else
                    return AppStatus.Unknown;
            }
        }

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

        private IList<string> _appDirectoryPaths;
        public IList<string> AppDirectoryPaths
        {
            get
            {
                if (_appDirectoryPaths == null)
                {
                    _appDirectoryPaths = new List<string>();

                    if (Paths?.Count > 0)
                    {
                        foreach (var path in Paths)
                        {
                            _appDirectoryPaths.Add(DirectoryUtility.GetDirectoryPath(path));
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(Path))
                    {
                        _appDirectoryPaths.Add(DirectoryUtility.GetDirectoryPath(Path));
                    }
                }

                return _appDirectoryPaths;
            }
        }
    }
}
