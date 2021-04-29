using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Regi.Abstractions
{
    public interface IProject : IService
    {
        string Name { get; set; }

        string Path { get; set; }

        IList<string> Paths { get; set; }

        IList<ProjectRole> Roles { get; set; }

        IList<string> Labels { get; set; }

        ProjectFramework Framework { get; set; }

        IDictionary<string, string> Commands { get; set; }

        IList<string> Requires { get; set; }

        CommandDictionary Arguments { get; set; }

        int? Port { get; set; }

        bool Serial { get; set; }

        string Source { get; set; }

        bool RawOutput { get; set; }

        CommandDictionary<object> Scripts { get; set; }

        bool Optional { get; set; }

        ConcurrentBag<IAppProcess> Processes { get; set; }

        ConcurrentBag<IProject> RequiredProjects { get; set; }

        AppStatus OutputStatus { get; }

        IList<string> GetAppDirectoryPaths(IFileSystem fileSystem);
    }
}
