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
        IList<Project> Start(CommandOptions options);
        IList<AppProcess> Test(CommandOptions options);
        IList<AppProcess> Install(CommandOptions options);
        OutputSummary List(CommandOptions options);
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

            FileInfo startupFile = directory.GetOneOfFiles("regi.json", "startup.json");

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

        private IDictionary<string, string> GetEnvironmentVariables(IList<Project> projects)
        {
            IDictionary<string, string> output = new Dictionary<string, string>();

            foreach (var p in projects)
            {
                if (p.Port.HasValue)
                {
                    output.Add($"{p.Name}_PORT".ToUnderscoreCase(), p.Port.ToString());
                }
            }

            return output;
        }

        public IList<Project> Start(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            IList<Project> projects = config.Apps.FilterByOptions(options);

            VariableList varList = new VariableList(projects);

            foreach (var project in projects)
            {
                _parallelService.Queue(() =>
                {
                    StartProject(project, new List<AppProcess>(), varList);
                });
            }

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var project in projects)
                {
                    project.Process.Dispose();
                }
            };

            _parallelService.RunInParallel();

            return projects;
        }

        private void StartProject(Project project, IList<AppProcess> processes, VariableList varList = null)
        {
            AppProcess process = null;
            if (project.Framework == ProjectFramework.Dotnet)
            {
                process = _dotnetService.RunProject(project, varList, false);
            }
            else if (project.Framework == ProjectFramework.Node)
            {
                process = _nodeService.StartProject(project, varList, false);
            }

            if (process != null)
            {
                project.Process = process;
                processes.Add(process);
            }
        }

        public IList<AppProcess> Test(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            IList<AppProcess> processes = new List<AppProcess>();

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var process in processes)
                {
                    process.Dispose();
                }
            };

            IList<Project> projects = config.Tests.FilterByOptions(options);

            VariableList varList = new VariableList(projects);

            foreach (var project in projects)
            {
                _parallelService.Queue(() =>
                {
                    if (project.Requires.Any())
                    {
                        foreach (var r in project.Requires)
                        {
                            Project requiredProject = config.Apps
                                .Concat(config.Services)
                                .FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));

                            if (requiredProject != null)
                            {
                                StartProject(requiredProject, processes, varList);
                            }
                        }
                    }

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
                    process.Dispose();
                }
            };

            _parallelService.RunInParallel();

            return processes;
        }

        public OutputSummary List(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            OutputSummary output = new OutputSummary();

            var apps = config.Apps.FilterByOptions(options);
            if (apps.Any())
            {
                _console.WriteEmphasizedLine("Apps:");
                foreach (var app in apps)
                {
                    output.Apps.Add(app);
                    _console.WriteLine("  " + app.Name);
                }
            }

            var tests = config.Tests.FilterByOptions(options);
            if (tests.Any())
            {
                _console.WriteEmphasizedLine("Tests:");
                foreach (var app in tests)
                {
                    output.Tests.Add(app);
                    _console.WriteLine("  " + app.Name);
                }
            }

            return output;
        }

        public void Initialize(CommandOptions options)
        {
            _fileService.CreateConfigFile();
        }
    }
}
