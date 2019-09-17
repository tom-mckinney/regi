﻿using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IRunnerService
    {
        Task<IList<Project>> StartAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken);
        Task<IList<Project>> TestAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken);
        Task<IList<Project>> BuildAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken);
        Task<IList<Project>> InstallAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken);
        Task KillAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken);
    }

    public class RunnerService : IRunnerService
    {
        private readonly IProjectManager _projectManager;
        private readonly IFrameworkServiceProvider _frameworkServiceProvider;
        private readonly IQueueService _queueService;
        private readonly INetworkingService _networkingService;
        private readonly IPlatformService _platformService;
        private readonly IConsole _console;

        public RunnerService(
            IProjectManager projectManager,
            IFrameworkServiceProvider frameworkServiceProvider,
            IQueueService queueService,
            INetworkingService networkingService,
            IPlatformService platformService,
            IConsole console)
        {
            _projectManager = projectManager;
            _frameworkServiceProvider = frameworkServiceProvider;
            _queueService = queueService;
            _networkingService = networkingService;
            _platformService = platformService;
            _console = console;
        }

        public async Task<IList<Project>> StartAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken)
        {
            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                RunScriptsForTask(project, AppTask.Start, options);

                foreach (var path in project.AppDirectoryPaths)
                {
                    _queueService.Queue(project.Serial || options.NoParallel, async () =>
                    {
                        await InternalStartProject(project, path, options, cancellationToken);
                    }, cancellationToken);
                }
            }

            await _queueService.RunAllAsync(cancellationToken);

            await _queueService.ConfirmProjectsStartedAsync(projects, cancellationToken);

            return projects;
        }

        private async Task<AppProcess> InternalStartProject(Project project, string applicationDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            _console.WriteEmphasizedLine($"Starting project {project.Name} ({applicationDirectoryPath})");

            AppProcess process = _frameworkServiceProvider
                .GetFrameworkService(project.Framework)
                .StartProject(project, applicationDirectoryPath, options);

            if (process != null)
            {
                project.Processes.Add(process);
            }

            await _queueService.WaitOnPortAsync(project, cancellationToken);

            return process;
        }

        public async Task<IList<Project>> TestAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken)
        {
            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                RunScriptsForTask(project, AppTask.Test, options);

                foreach (var path in project.AppDirectoryPaths)
                {
                    _queueService.Queue(project.Serial || options.NoParallel, async () =>
                    {
                        string appName = project.AppDirectoryPaths.Count > 1 ? $"{DirectoryUtility.GetDirectoryShortName(path)} ({project.Name})" : project.Name;
                        _console.WriteEmphasizedLine($"Starting tests for {appName}");

                        if (project.RequiredProjects.Any())
                        {
                            string dependencyPluralization = project.RequiredProjects.Count > 1 ? "dependencies" : "dependency";
                            _console.WriteDefaultLine($"Starting {project.RequiredProjects.Count} {dependencyPluralization} for project {appName}");

                            RegiOptions requiredOptions = options.CloneForRequiredProjects();

                            IQueueService dependencyQueue = _frameworkServiceProvider.CreateScopedQueueService();

                            IDictionary<int, Project> requiredProjectsWithPorts = new Dictionary<int, Project>();

                            foreach (var requiredProject in project.RequiredProjects)
                            {
                                RunScriptsForTask(requiredProject, AppTask.Test, options);

                                if (requiredProject != null)
                                {
                                    if (requiredProject.Port.HasValue)
                                    {
                                        if (_networkingService.IsPortListening(requiredProject.Port.Value))
                                        {
                                            _console.WriteWarningLine($"Project {requiredProject.Name} is already listening on port {requiredProject.Port}");
                                            continue;
                                        }

                                        requiredProjectsWithPorts.Add(requiredProject.Port.Value, requiredProject);
                                    }

                                    bool noParallel = requiredProject.Serial || options.NoParallel;

                                    foreach (var requiredPath in requiredProject.AppDirectoryPaths)
                                    {
                                        dependencyQueue.Queue(noParallel, async () =>
                                        {
                                            await InternalStartProject(requiredProject, requiredPath, requiredOptions, cancellationToken);
                                        }, cancellationToken);
                                    }
                                }
                            }

                            await dependencyQueue.RunAllAsync(cancellationToken);
                            await dependencyQueue.ConfirmProjectsStartedAsync(requiredProjectsWithPorts, cancellationToken);
                        }

                        var testProcess = _frameworkServiceProvider
                            .GetFrameworkService(project.Framework)
                            .TestProject(project, path, options);

                        project.Processes.Add(testProcess);

                        if (project.Processes?.Count > 0)
                        {
                            var outputStatus = project.OutputStatus;
                            string outputMessage = $"Finished tests for {appName} with status {outputStatus}";

                            if (outputStatus == AppStatus.Success)
                                _console.WriteSuccessLine(outputMessage);
                            else
                                _console.WriteErrorLine(outputMessage);

                            _projectManager.KillAllProcesses(project.RequiredProjects, options);
                        }
                    }, cancellationToken);
                }
            }

            await _queueService.RunAllAsync(cancellationToken);

            return projects;
        }

        public async Task<IList<Project>> BuildAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken)
        {
            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                foreach (var path in project.AppDirectoryPaths)
                {
                    _queueService.Queue(project.Serial || options.NoParallel, () =>
                    {
                        _console.WriteEmphasizedLine($"Starting build for project {project.Name}");

                        var buildProcess = _frameworkServiceProvider
                            .GetFrameworkService(project.Framework)
                            .BuildProject(project, path, options);

                        project.Processes.Add(buildProcess);

                        if (project.OutputStatus == AppStatus.Success)
                        {
                            _console.WriteSuccessLine($"Finished build for project {project.Name}", ConsoleLineStyle.LineAfter);
                        }
                        else if (project.OutputStatus != AppStatus.Unknown)
                        {
                            _console.WriteErrorLine($"Build for project {project.Name} exited with status {project.OutputStatus}", ConsoleLineStyle.LineAfter);
                        }
                    }, cancellationToken);
                }
            }

            await _queueService.RunAllAsync(cancellationToken);

            return projects;
        }

        public async Task<IList<Project>> InstallAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken)
        {
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
                            InternalInstallProject(project, path, options);
                        }

                        if (project.RequiredProjects.Any())
                        {
                            string dependencyPluralization = project.Requires.Count > 1 ? "dependencies" : "dependency";
                            _console.WriteDefaultLine($"Starting install for {project.RequiredProjects.Count} {dependencyPluralization} for project {project.Name}");

                            RegiOptions requiredOptions = options.CloneForRequiredProjects();

                            IQueueService dependencyQueue = _frameworkServiceProvider.CreateScopedQueueService();

                            foreach (var requiredProject in project.RequiredProjects)
                            {
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
                                            InternalInstallProject(requiredProject, requiredPath, requiredOptions);
                                        }, cancellationToken);
                                    }
                                }
                            }

                            dependencyQueue.RunAllAsync(cancellationToken).Wait(cancellationToken);
                        }
                    }, cancellationToken);
                }
            }

            await _queueService.RunAllAsync(cancellationToken);

            return projects;
        }

        private AppProcess InternalInstallProject(Project project, string appDirectoryPath, RegiOptions options)
        {
            _console.WriteEmphasizedLine($"Starting install for project {project.Name}");

            var process = _frameworkServiceProvider
                .GetFrameworkService(project.Framework)
                .InstallProject(project, appDirectoryPath, options);

            _console.WriteSuccessLine($"Finished installing project {project.Name}");

            project.Processes.Add(process);

            return process;
        }

        public Task KillAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken)
        {
            _console.WriteEmphasizedLine("Committing regicide...");

            IEnumerable<ProjectFramework> frameworks;
            if (projects?.Count > 0)
            {
                frameworks = projects.Select(p => p.Framework).Distinct();
            }
            else
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

            return Task.CompletedTask;
        }

        private void RunScriptsForTask(Project project, AppTask task, RegiOptions options)
        {
            if (project.Scripts?.Count > 0
                    && project.Scripts.TryGetValue(task, out IList<object> beforeScripts))
            {
                foreach (var script in beforeScripts)
                {
                    if (script is string simpleScript)
                    {
                        _platformService.RunAnonymousScript(simpleScript, options);
                    }
                    else if (script is AppScript appScript)
                    {
                        _platformService.RunAnonymousScript(appScript.Run, options, appScript.Path);
                    }
                }
            }
        }
    }
}
