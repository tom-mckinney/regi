using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Regi.Services
{
    public interface IRunnerService
    {
        StartupConfig GetStartupConfig();
        IList<Project> Start(CommandOptions options);
        IList<Project> Test(CommandOptions options);
        IList<Project> Install(CommandOptions options);
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
                throw new FileNotFoundException($"Could not find regi.json or startup.json in directory: {directory.FullName}");
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

            options.VariableList = new VariableList(config);

            IList<Project> projects = config.Apps.FilterByOptions(options);

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var project in projects)
                {
                    project.Process?.Dispose();
                }
            };

            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                _parallelService.Queue(project.Serial || options.NoParallel, () =>
                {
                    StartProject(project, null, options);
                });
            }

            _parallelService.RunAll();

            _parallelService.WaitOnPorts(projects);

            return projects;
        }

        private AppProcess StartProject(Project project, IList<Project> projects, CommandOptions options)
        {
            _console.WriteEmphasizedLine($"Starting project {project.Name} ({project.File.DirectoryName})");

            AppProcess process = null;
            if (project.Framework == ProjectFramework.Dotnet)
            {
                process = _dotnetService.StartProject(project, options);
            }
            else if (project.Framework == ProjectFramework.Node)
            {
                process = _nodeService.StartProject(project, options);
            }

            if (process != null)
            {
                project.Process = process;
                if (projects != null)
                {
                    projects.Add(project);
                }
            }

            return process;
        }

        public IList<Project> Test(CommandOptions options)
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

            options.VariableList = new VariableList(config);

            IList<Project> projects = config.Tests.FilterByOptions(options);

            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                _parallelService.Queue(project.Serial || options.NoParallel, () =>
                {
                    IList<AppProcess> associatedProcesses = new List<AppProcess>();

                    if (project.Requires.Any())
                    {
                        IDictionary<int, Project> requiredProjectsWithPorts = new Dictionary<int, Project>();

                        foreach (var r in project.Requires)
                        {
                            Project requiredProject = config.Apps
                                .Concat(config.Services)
                                .FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));

                            if (requiredProject != null)
                            {
                                if (requiredProject.Port.HasValue)
                                    requiredProjectsWithPorts.Add(requiredProject.Port.Value, requiredProject);

                                var requiredProccess = StartProject(requiredProject, projects, options);
                                associatedProcesses.Add(requiredProccess);
                            }
                        }

                        _parallelService.WaitOnPorts(requiredProjectsWithPorts);
                    }

                    _console.WriteEmphasizedLine($"Starting tests for project {project.Name}");

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        project.Process = _dotnetService.TestProject(project, options);
                    }
                    else if (project.Framework == ProjectFramework.Node)
                    {
                        project.Process = _nodeService.TestProject(project, options);
                    }

                    if (project.Process != null)
                    {
                        string outputMessage = $"Finished tests for project {project.Name} with status {project.Process?.Status}";
                        if (project.Process?.Status == AppStatus.Success)
                            _console.WriteSuccessLine(outputMessage, ConsoleLineStyle.LineBeforeAndAfter);
                        else
                            _console.WriteErrorLine(outputMessage, ConsoleLineStyle.LineBeforeAndAfter);

                        processes.Add(project.Process);

                        associatedProcesses.DisposeAll();
                    }
                });
            }

            _parallelService.RunAll();

            processes.DisposeAll();

            return projects;
        }

        public IList<Project> Install(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            IList<Project> projects = config.Apps.Concat(config.Tests).FilterByOptions(options);

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var project in projects)
                {
                    project.Process?.Dispose();
                }
            };

            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                _parallelService.QueueParallel(() =>
                {
                    _console.WriteEmphasizedLine($"Starting install for project {project.Name}");

                    AppProcess process = null;

                    project.TryAddSource(options, config);

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        process = _dotnetService.InstallProject(project, options);
                    }
                    else if (project.Framework == ProjectFramework.Node)
                    {
                        process = _nodeService.InstallProject(project, options);
                    }

                    if (process != null)
                    {
                        project.Process = process;
                    }

                });
            }

            _parallelService.RunAll();

            return projects;
        }

        public void Initialize(CommandOptions options)
        {
            _fileService.CreateConfigFile();
        }

        public OutputSummary List(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            OutputSummary output = new OutputSummary();

            var apps = config.Apps.FilterByOptions(options);
            var tests = config.Tests.FilterByOptions(options);

            PrintAppGroupDetails(apps, output.Apps, "Apps");
            PrintAppGroupDetails(tests, output.Tests, "Tests");

            void PrintAppGroupDetails(IList<Project> inputApps, IList<Project> outputApps, string groupName)
            {
                if (inputApps != null && inputApps.Any())
                {
                    _console.WriteEmphasizedLine($"{groupName}:");
                    foreach (var app in inputApps)
                    {
                        outputApps.Add(app);

                        _console.WriteLine("  " + app.Name);

                        if (options.Verbose)
                        {
                            _console.WritePropertyIfSpecified("Framework", app.Framework);
                            _console.WritePropertyIfSpecified("Path", app.Path);
                            _console.WritePropertyIfSpecified("Port", app.Port);
                            _console.WritePropertyIfSpecified("Commands", app.Commands);
                            _console.WritePropertyIfSpecified("Requires", app.Requires);
                            _console.WritePropertyIfSpecified("Options", app.Options);
                            _console.WritePropertyIfSpecified("Environment", app.Environment);
                        }
                    }
                }
            }

            return output;
        }
    }
}
