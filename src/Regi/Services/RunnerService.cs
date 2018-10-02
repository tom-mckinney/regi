using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Regi.Services
{
    public interface IRunnerService
    {
        StartupConfig GetStartupConfig();
        IList<AppProcess> Start(CommandOptions options);
        IList<AppProcess> Test(CommandOptions options);
        IList<AppProcess> Install(CommandOptions options);
        void Initialize(CommandOptions options);
    }

    public class RunnerService : IRunnerService
    {
        private readonly IDotnetService _dotnetService;
        private readonly INodeService _nodeService;
        private readonly IParallelService _parallelService;
        private readonly IFileService _fileService;
        private readonly IConsole _console;

        public RunnerService(IDotnetService dotnetService,
            INodeService nodeService,
            IParallelService parallelService,
            IFileService fileService,
            IConsole console)
        {
            _dotnetService = dotnetService;
            _nodeService = nodeService;
            _parallelService = parallelService;
            _fileService = fileService;
            _console = console;
        }

        public StartupConfig GetStartupConfig()
        {
            DirectoryInfo directory = new DirectoryInfo(DirectoryUtility.TargetDirectoryPath);

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
                    throw new JsonSerializationException($"Configuration file was not properly formatted: {startupFile.FullName}{Environment.NewLine}{e.Message}", e);
                }
            }
        }

        public IList<AppProcess> Start(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            IList<AppProcess> processes = new List<AppProcess>();

            foreach (var project in config.Apps.FilterByOptions(options))
            {
                _parallelService.Queue(() =>
                {
                    AppProcess process = null;

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        process = _dotnetService.RunProject(project, false, project.Port);
                    }
                    else if (project.Framework == ProjectFramework.Node)
                    {
                        process = _nodeService.StartProject(project, false, project.Port);
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

        public IList<AppProcess> Test(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            IList<AppProcess> processes = new List<AppProcess>();

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var process in processes)
                {
                    process.Process.KillTree(TimeSpan.FromSeconds(10));
                }
            };

            foreach (var project in config.Tests.FilterByOptions(options))
            {
                _parallelService.Queue(() =>
                {
                    AppProcess process = null;

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        _console.WriteLine(project.File.FullName);
                        process = _dotnetService.TestProject(project);
                    }
                    else if (project.Framework == ProjectFramework.Node)
                    {
                        process = _nodeService.TestProject(project);
                    }

                    if (process != null)
                    {
                        processes.Add(process);
                    }
                });
            }

            _parallelService.RunInParallel();

            return processes;
        }

        public IList<AppProcess> Install(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            IList<AppProcess> processes = new List<AppProcess>();

            IList<Project> appsAndTests = config.Apps.Concat(config.Tests).ToList();
            foreach (var project in appsAndTests.FilterByOptions(options))
            {
                _parallelService.Queue(() =>
                {
                    AppProcess process = null;

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        process = _dotnetService.RestoreProject(project, false);
                    }
                    else if (project.Framework == ProjectFramework.Node)
                    {
                        process = _nodeService.InstallProject(project, false);
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

        public void Initialize(CommandOptions options)
        {
            _fileService.CreateConfigFile();
        }
    }
}
