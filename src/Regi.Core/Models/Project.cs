using Regi.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Regi.Models
{
    public class Project : IProject
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

        public IList<ProjectRole> Roles { get; set; } = new List<ProjectRole>();

        public IList<string> Labels { get; set; } = new List<string>();

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

        [JsonIgnore]
        public ConcurrentBag<IAppProcess> Processes { get; set; } = new ConcurrentBag<IAppProcess>();

        [JsonIgnore]
        public ConcurrentBag<IProject> RequiredProjects { get; set; } = new ConcurrentBag<IProject>();

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

        public void TryAddSource(CommandOptions options, ServiceMesh config)
        {
            if (string.IsNullOrWhiteSpace(Source))
            {
                if (!string.IsNullOrWhiteSpace(options.Source))
                {
                    Source = options.Source;
                }
                else if (config.GetSources()?.Count > 0
                    && (config.GetSources().TryGetValue(Framework, out string source) || config.GetSources().TryGetValue(ProjectFramework.Any, out source))
                    && !string.IsNullOrWhiteSpace(source))
                {
                    Source = source;
                }
            }
        }

        private IList<string> _appDirectoryPaths;
        public IList<string> GetAppDirectoryPaths(IFileSystem fileSystem)
        {
            if (_appDirectoryPaths == null)
            {
                _appDirectoryPaths = new List<string>();

                if (Paths?.Count > 0)
                {
                    foreach (var path in Paths)
                    {
                        _appDirectoryPaths.Add(fileSystem.GetDirectoryPath(path));
                    }
                }
                else if (!string.IsNullOrWhiteSpace(Path))
                {
                    _appDirectoryPaths.Add(fileSystem.GetDirectoryPath(Path));
                }
            }

            return _appDirectoryPaths;
        }
    }
}
