using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Regi.Services
{
    public interface IRunnerService
    {
        StartupConfig GetStartupConfig(string path);
        IList<AppProcess> RunAsync(string directoryName);
        IList<AppProcess> TestAsync(string directoryName, ProjectType? type = null);
    }

    public class RunnerService : IRunnerService
    {
        private readonly IDotnetService _dotnetService;
        private readonly INodeService _nodeService;
        private readonly IConsole _console;

        public RunnerService(IDotnetService dotnetService, INodeService nodeService, IConsole console)
        {
            _dotnetService = dotnetService;
            _nodeService = nodeService;
            _console = console;
        }

        public StartupConfig GetStartupConfig(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"Could not find directory: {directory.FullName}");
            }

            FileInfo startupFile = directory.GetFiles("startup.json").FirstOrDefault();

            if (startupFile == null || !startupFile.Exists)
            {
                throw new FileNotFoundException($"Could not find startup.json in directory: {directory.FullName}");
            }

            using (StreamReader sr = new StreamReader(startupFile.OpenRead()))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = JsonSerializer.CreateDefault();
                serializer.MissingMemberHandling = MissingMemberHandling.Error;

                return serializer.Deserialize<StartupConfig>(reader);
            }
        }

        public IList<AppProcess> RunAsync(string directoryName)
        {
            StartupConfig config = GetStartupConfig(directoryName);

            IList<AppProcess> processes = new List<AppProcess>();


            foreach (var project in config.Apps)
            {
                if (project.Type == ProjectType.Web)
                {
                    string absolutePath = Path.GetFullPath(project.Path, directoryName);
                    FileInfo projectFile = new FileInfo(absolutePath);

                    if (projectFile.Exists)
                    {
                        AppProcess process = null;

                        if (project.Framework == ProjectFramework.Dotnet)
                        {
                            process = _dotnetService.RunProject(projectFile, false, project.Port);
                        }
                        else if (project.Framework == ProjectFramework.Node)
                        {
                            process = _nodeService.StartProject(projectFile, false, project.Port);
                        }

                        if (process != null)
                        {
                            processes.Add(process);
                        }
                    }
                }
            }

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var process in processes)
                {
                    process.Process.KillTree(TimeSpan.FromSeconds(10));
                }
            };

            return processes;
        }

        public IList<AppProcess> TestAsync(string directoryName, ProjectType? type = null)
        {
            StartupConfig config = GetStartupConfig(directoryName);

            IList<AppProcess> processes = new List<AppProcess>();

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var process in processes)
                {
                    process.Process.KillTree(TimeSpan.FromSeconds(10));
                }
            };

            foreach (var project in config.Tests)
            {
                if (!type.HasValue || project.Type == type)
                {
                    string absolutePath = Path.GetFullPath(project.Path, directoryName);
                    FileInfo projectFile = new FileInfo(absolutePath);

                    if (projectFile.Exists)
                    {
                        _console.WriteLine(projectFile.FullName);
                        processes.Add(_dotnetService.TestProject(projectFile));
                    }
                }
            }

            return processes;
        }
    }
}
