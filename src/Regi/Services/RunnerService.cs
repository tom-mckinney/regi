using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Regi.Services
{
    public interface IRunnerService
    {
        StartupConfig GetStartupConfig(string path);
        IList<AppProcess> Run(string directoryName);
        IList<AppProcess> Test(string directoryName, ProjectType? type = null);
        IList<AppProcess> Install(string directoryName);
    }

    public class RunnerService : IRunnerService
    {
        private readonly IDotnetService _dotnetService;
        private readonly INodeService _nodeService;
        private readonly IParallelService _parallelService;
        private readonly IConsole _console;

        public RunnerService(IDotnetService dotnetService, INodeService nodeService, IParallelService parallelService, IConsole console)
        {
            _dotnetService = dotnetService;
            _nodeService = nodeService;
            _parallelService = parallelService;
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

                try
                {
                    return serializer.Deserialize<StartupConfig>(reader);
                }
                catch (Exception e)
                {
                    throw new JsonSerializationException($@"Configuration file was not properly formatted: {startupFile.FullName}
{e.Message}", e);
                }
            }
        }

        public IList<AppProcess> Install(string directoryName)
        {
            StartupConfig config = GetStartupConfig(directoryName);

            IList<AppProcess> processes = new List<AppProcess>();

            foreach (var project in config.Apps.Concat(config.Tests))
            {
                _parallelService.Queue(() =>
                {
                    string absolutePath = Path.GetFullPath(project.Path, directoryName);
                    FileInfo projectFile = new FileInfo(absolutePath);

                    if (!projectFile.Exists)
                    {
                        throw new FileNotFoundException($"Could not find project, {projectFile.FullName}");
                    }

                    AppProcess process = null;

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        process = _dotnetService.RestoreProject(projectFile, false);
                    }
                    else if (project.Framework == ProjectFramework.Node)
                    {
                        process = _nodeService.InstallProject(projectFile, false);
                    }

                    if (process != null)
                    {
                        processes.Add(process);
                    }

                });
            }

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var process in processes)
                {
                    process.Process.KillTree(TimeSpan.FromSeconds(10));
                }
            };

            _parallelService.RunInParallel();

            return processes;
        }

        public IList<AppProcess> Run(string directoryName)
        {
            StartupConfig config = GetStartupConfig(directoryName);

            IList<AppProcess> processes = new List<AppProcess>();

            foreach (var project in config.Apps)
            {
                if (project.Type == ProjectType.Web)
                {
                    _parallelService.Queue(() =>
                    {
                        string absolutePath = Path.GetFullPath(project.Path, directoryName);
                        FileInfo projectFile = new FileInfo(absolutePath);

                        if (!projectFile.Exists)
                        {
                            throw new FileNotFoundException($"Could not find project, {projectFile.FullName}");
                        }

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
                    });
                }
            }

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var process in processes)
                {
                    process.Process.KillTree(TimeSpan.FromSeconds(10));
                }
            };

            _parallelService.RunInParallel();

            return processes;
        }

        public IList<AppProcess> Test(string directoryName, ProjectType? type = null)
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
                    _parallelService.Queue(() =>
                    {
                        string absolutePath = Path.GetFullPath(project.Path, directoryName);
                        FileInfo projectFile = new FileInfo(absolutePath);

                        if (!projectFile.Exists)
                        {
                            throw new FileNotFoundException($"Could not find project, {projectFile.FullName}");
                        }

                        AppProcess process = null;

                        if (project.Framework == ProjectFramework.Dotnet)
                        {
                            _console.WriteLine(projectFile.FullName);
                            process = _dotnetService.TestProject(projectFile);
                        }
                        else if (project.Framework == ProjectFramework.Node)
                        {
                            process = _nodeService.TestProject(projectFile);
                        }

                        if (process != null)
                        {
                            processes.Add(process);
                        }
                    });
                }
            }

            _parallelService.RunInParallel();

            return processes;
        }
    }
}
