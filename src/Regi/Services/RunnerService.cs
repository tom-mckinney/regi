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
        IList<Project> Start(RegiOptions options);
        IList<Project> Test(RegiOptions options);
        IList<Project> Build(RegiOptions options);
        IList<Project> Install(RegiOptions options);
        OutputSummary List(RegiOptions options);
        void Kill(RegiOptions options);
        void Initialize(RegiOptions options);
    }

    public class RunnerService : IRunnerService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IFrameworkServiceProvider _frameworkServiceProvider;
        private readonly IQueueService _queueService;
        private readonly INetworkingService _networkingService;
        private readonly IFileService _fileService;
        private readonly IConsole _console;

        public RunnerService(IConfigurationService configurationService,
            IFrameworkServiceProvider frameworkServiceProvider,
            IQueueService queueService,
            INetworkingService networkingService,
            IFileService fileService,
            IConsole console)
        {
            _configurationService = configurationService;
            _frameworkServiceProvider = frameworkServiceProvider;
            _queueService = queueService;
            _networkingService = networkingService;
            _fileService = fileService;
            _console = console;
        }

        public IList<Project> Start(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

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
                _queueService.Queue(project.Serial || options.NoParallel, () =>
                {
                    InternalStartProject(project, options);
                });
            }

            _queueService.RunAll();

            _queueService.WaitOnPorts(projects);

            return projects;
        }

        private AppProcess InternalStartProject(Project project, RegiOptions options)
        {
            _console.WriteEmphasizedLine($"Starting project {project.Name} ({project.File.DirectoryName})");

            AppProcess process = _frameworkServiceProvider
                .GetFrameworkService(project.Framework)
                .StartProject(project, options);

            if (process != null)
            {
                project.Process = process;
            }

            return process;
        }

        public IList<Project> Test(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

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
                _queueService.Queue(project.Serial || options.NoParallel, () =>
                {
                    RegiOptions requiredOptions = options.CloneForRequiredProjects();

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
                                {
                                    if (_networkingService.IsPortListening(requiredProject.Port.Value))
                                    {
                                        _console.WriteLine($"Project {requiredProject.Name} is already listening on port {requiredProject.Port}");
                                        continue;
                                    }

                                    requiredProjectsWithPorts.Add(requiredProject.Port.Value, requiredProject);
                                }

                                requiredProject.Process = InternalStartProject(requiredProject, requiredOptions);

                                project.RequiredProjects.Add(requiredProject);
                            }
                        }

                        _queueService.WaitOnPorts(requiredProjectsWithPorts);
                    }

                    _console.WriteEmphasizedLine($"Starting tests for project {project.Name}");

                    project.Process = _frameworkServiceProvider
                        .GetFrameworkService(project.Framework)
                        .TestProject(project, options);

                    if (project.Process != null)
                    {
                        string outputMessage = $"Finished tests for project {project.Name} with status {project.Process?.Status}";
                        if (project.Process?.Status == AppStatus.Success)
                            _console.WriteSuccessLine(outputMessage, ConsoleLineStyle.LineBeforeAndAfter);
                        else
                            _console.WriteErrorLine(outputMessage, ConsoleLineStyle.LineBeforeAndAfter);

                        processes.Add(project.Process);

                        foreach (var p in project.RequiredProjects)
                        {
                            p.Process.Dispose();
                        }
                    }
                });
            }

            _queueService.RunAll();

            processes.DisposeAll();

            return projects;
        }

        public IList<Project> Build(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

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
                _queueService.QueueSerial(() =>
                {
                    _console.WriteEmphasizedLine($"Starting build for project {project.Name}");

                    project.Process = _frameworkServiceProvider
                        .GetFrameworkService(project.Framework)
                        .BuildProject(project, options);

                    if (project.Process?.Status == AppStatus.Success)
                    {
                        _console.WriteSuccessLine($"Finished build for project {project.Name}", ConsoleLineStyle.LineAfter);
                    }
                });
            }

            _queueService.RunAll();

            return projects;
        }

        public IList<Project> Install(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

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
                _queueService.QueueParallel(() =>
                {
                    _console.WriteEmphasizedLine($"Starting install for project {project.Name}");

                    project.TryAddSource(options, config);

                    project.Process = _frameworkServiceProvider
                        .GetFrameworkService(project.Framework)
                        .InstallProject(project, options);
                });
            }

            _queueService.RunAll();

            return projects;
        }

        public void Initialize(RegiOptions options)
        {
            _fileService.CreateConfigFile();
        }

        public OutputSummary List(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

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

        public void Kill(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

            IList<Project> projects = config.Apps
                .Concat(config.Tests)
                .FilterByOptions(options);

            IEnumerable<ProjectFramework> frameworks = projects.Select(p => p.Framework).Distinct();

            foreach (var framework in frameworks)
            {
                _console.WriteEmphasizedLine($"Killing processes for framework: {framework}", ConsoleLineStyle.LineBefore);

                _frameworkServiceProvider
                    .GetFrameworkService(framework)
                    .KillProcesses(options);
            }

            _console.WriteSuccessLine("Finished killing processess successfuly", ConsoleLineStyle.LineBeforeAndAfter);
        }
    }
}
