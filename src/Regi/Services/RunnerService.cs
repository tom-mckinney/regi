using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Concurrent;
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
        private readonly IProjectManager _projectManager;
        private readonly IConfigurationService _configurationService;
        private readonly IFrameworkServiceProvider _frameworkServiceProvider;
        private readonly IQueueService _queueService;
        private readonly INetworkingService _networkingService;
        private readonly IFileService _fileService;
        private readonly IConsole _console;

        public RunnerService(
            IProjectManager projectManager,
            IConfigurationService configurationService,
            IFrameworkServiceProvider frameworkServiceProvider,
            IQueueService queueService,
            INetworkingService networkingService,
            IFileService fileService,
            IConsole console)
        {
            _projectManager = projectManager;
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

            IList<Project> projects = _projectManager.FilterAndTrackProjects(options, config.Apps);

            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                foreach (var path in project.AppDirectoryPaths)
                {
                    _queueService.Queue(project.Serial || options.NoParallel, () =>
                    {
                        InternalStartProject(project, path, options);
                    });
                }
            }

            _queueService.RunAll();

            _queueService.ConfirmProjectsStarted(projects);

            return projects;
        }

        private AppProcess InternalStartProject(Project project, string applicationDirectoryPath, RegiOptions options)
        {
            _console.WriteEmphasizedLine($"Starting project {project.Name} ({applicationDirectoryPath})");

            AppProcess process = _frameworkServiceProvider
                .GetFrameworkService(project.Framework)
                .StartProject(project, applicationDirectoryPath, options);

            if (process != null)
            {
                project.Processes.Add(process);
            }

            _queueService.WaitOnPort(project);

            return process;
        }

        public IList<Project> Test(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

            options.VariableList = new VariableList(config);

            IList<Project> projects = _projectManager.FilterAndTrackProjects(options, config.Tests);

            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                foreach (var path in project.AppDirectoryPaths)
                {
                    _queueService.Queue(project.Serial || options.NoParallel, () =>
                    {
                        _console.WriteEmphasizedLine($"Starting tests for project {project.Name}");

                        if (project.Requires.Any())
                        {
                            string dependencyPluralization = project.Requires.Count > 1 ? "dependencies" : "dependency";
                            _console.WriteDefaultLine($"Starting {project.Requires.Count} {dependencyPluralization} for project {project.Name}");

                            RegiOptions requiredOptions = options.CloneForRequiredProjects();

                            IQueueService dependencyQueue = _frameworkServiceProvider.CreateScopedQueueService();

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

                                    bool noParallel = requiredProject.Serial || options.NoParallel;

                                    project.RequiredProjects.Add(requiredProject);

                                    foreach (var requiredPath in requiredProject.AppDirectoryPaths)
                                    {
                                        dependencyQueue.Queue(noParallel, () =>
                                        {
                                            InternalStartProject(requiredProject, requiredPath, requiredOptions);
                                        });
                                    }
                                }
                            }

                            dependencyQueue.RunAll();
                            dependencyQueue.ConfirmProjectsStarted(requiredProjectsWithPorts);
                        }

                        var testProcess = _frameworkServiceProvider
                            .GetFrameworkService(project.Framework)
                            .TestProject(project, path, options);

                        project.Processes.Add(testProcess);

                        if (project.Processes?.Count > 0)
                        {
                            var outputStatus = project.OutputStatus;
                            string outputMessage = $"Finished tests for project {project.Name} with status {outputStatus}";

                            if (outputStatus == AppStatus.Success)
                                _console.WriteSuccessLine(outputMessage, ConsoleLineStyle.LineBeforeAndAfter);
                            else
                                _console.WriteErrorLine(outputMessage, ConsoleLineStyle.LineBeforeAndAfter);

                            _projectManager.KillAllProcesses(project.RequiredProjects, options);
                        }
                    });
                }
            }

            _queueService.RunAll();

            _projectManager.KillAllProcesses(options); // TODO: should this be done as part of global command lifecycle?

            return projects;
        }

        public IList<Project> Build(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

            options.VariableList = new VariableList(config);

            IList<Project> projects = _projectManager.FilterAndTrackProjects(options, config.Apps);

            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                foreach (var path in project.AppDirectoryPaths)
                {
                    _queueService.QueueSerial(() =>
                    {
                        _console.WriteEmphasizedLine($"Starting build for project {project.Name}");

                        project.Processes.Add(_frameworkServiceProvider
                            .GetFrameworkService(project.Framework)
                            .BuildProject(project, path, options));

                        if (project.OutputStatus == AppStatus.Success)
                        {
                            _console.WriteSuccessLine($"Finished build for project {project.Name}", ConsoleLineStyle.LineAfter);
                        }
                        else
                        {
                            _console.WriteErrorLine($"Build for project {project.Name} exited with status {project.OutputStatus}", ConsoleLineStyle.LineAfter);
                        }
                    });
                }
            }

            _queueService.RunAll();

            return projects;
        }

        public IList<Project> Install(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

            IList<Project> projects = _projectManager.FilterAndTrackProjects(options, config.Apps, config.Tests);

            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            var installedProjects = new ConcurrentBag<string>();

            foreach (var project in projects)
            {
                foreach (var path in project.AppDirectoryPaths)
                {
                    _queueService.Queue(project.Serial || options.NoParallel, () =>
                    {
                        if (!installedProjects.Contains(project.Name))
                        {
                            installedProjects.Add(project.Name);
                            InternalInstallProject(project, path, options, config); // TODO: Loop through all paths
                        }

                        if (project.Requires.Any())
                        {
                            string dependencyPluralization = project.Requires.Count > 1 ? "dependencies" : "dependency";
                            _console.WriteDefaultLine($"Starting install for {project.Requires.Count} {dependencyPluralization} for project {project.Name}");

                            RegiOptions requiredOptions = options.CloneForRequiredProjects();

                            IQueueService dependencyQueue = _frameworkServiceProvider.CreateScopedQueueService();

                            foreach (var r in project.Requires)
                            {
                                Project requiredProject = config.Apps
                                    .Concat(config.Services)
                                    .FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));

                                if (requiredProject != null)
                                {
                                    if (installedProjects.Contains(requiredProject.Name) || projects.Any(p => p.Name == requiredProject.Name))
                                    {
                                        continue;
                                    }

                                    installedProjects.Add(requiredProject.Name);

                                    foreach (var requiredPath in requiredProject.AppDirectoryPaths)
                                    {
                                        dependencyQueue.Queue(requiredProject.Serial || options.NoParallel, () =>
                                        {
                                            InternalInstallProject(requiredProject, requiredPath, requiredOptions, config);

                                            project.RequiredProjects.Add(requiredProject);
                                        });
                                    }
                                }
                            }

                            dependencyQueue.RunAll();
                        }
                    });
                }
            }

            _queueService.RunAll();

            return projects;
        }

        private AppProcess InternalInstallProject(Project project, string appDirectoryPath, RegiOptions options, StartupConfig config)
        {
            _console.WriteEmphasizedLine($"Starting install for project {project.Name}");

            project.TryAddSource(options, config);

            var process = _frameworkServiceProvider
                .GetFrameworkService(project.Framework)
                .InstallProject(project, appDirectoryPath, options);

            _console.WriteSuccessLine($"Finished installing project {project.Name}");

            project.Processes.Add(process);

            return process;
        }

        public void Initialize(RegiOptions options)
        {
            _fileService.CreateConfigFile();
        }

        public OutputSummary List(RegiOptions options)
        {
            StartupConfig config = _configurationService.GetConfiguration();

            OutputSummary output = new OutputSummary();

            var apps = _projectManager.FilterByOptions(config.Apps, options);
            var tests = _projectManager.FilterByOptions(config.Tests, options);

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
                            _console.WritePropertyIfSpecified("Type", app.Type);
                            _console.WritePropertyIfSpecified("Path", app.Path);
                            _console.WritePropertyIfSpecified("Port", app.Port);
                            _console.WritePropertyIfSpecified("Commands", app.Commands);
                            _console.WritePropertyIfSpecified("Requires", app.Requires);
                            _console.WritePropertyIfSpecified("Options", app.Options);
                            _console.WritePropertyIfSpecified("Environment", app.Environment);
                            _console.WritePropertyIfSpecified("Serial", app.Serial);
                            _console.WritePropertyIfSpecified("Raw Output", app.RawOutput);
                        }
                    }
                }
            }

            return output;
        }

        public void Kill(RegiOptions options)
        {
            _console.WriteEmphasizedLine("Committing regicide...");

            IEnumerable<ProjectFramework> frameworks;
            try
            {
                StartupConfig config = _configurationService.GetConfiguration();

                IList<Project> projects = _projectManager.FilterAndTrackProjects(options, config.Apps, config.Tests);

                frameworks = projects.Select(p => p.Framework).Distinct();
            }
            catch (IOException)
            {
                frameworks = _frameworkServiceProvider.GetAllProjectFrameworkTypes();
            }

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
