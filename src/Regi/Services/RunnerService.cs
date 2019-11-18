using McMaster.Extensions.CommandLineUtils;
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
        private readonly IFileSystem _fileSystem;
        private readonly IConsole _console;

        public RunnerService(
            IProjectManager projectManager,
            IFrameworkServiceProvider frameworkServiceProvider,
            IQueueService queueService,
            INetworkingService networkingService,
            IPlatformService platformService,
            IFileSystem fileSystem,
            IConsole console)
        {
            _projectManager = projectManager;
            _frameworkServiceProvider = frameworkServiceProvider;
            _queueService = queueService;
            _networkingService = networkingService;
            _platformService = platformService;
            _fileSystem = fileSystem;
            _console = console;
        }

        public async Task<IList<Project>> StartAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken)
        {
            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                RunScriptsForTask(project, AppTask.Start, options);

                foreach (var path in project.GetAppDirectoryPaths(_fileSystem))
                {
                    _queueService.Queue(project.Serial || options.NoParallel, () =>
                    {
                        return InternalStartProject(project, path, options, cancellationToken);
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

            AppProcess process = await _frameworkServiceProvider
                .GetFrameworkService(project.Framework)
                .StartProject(project, applicationDirectoryPath, options, cancellationToken);

            if (process != null)
            {
                project.Processes.Add(process);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _queueService.WaitOnPortAsync(project, cancellationToken);

            return process;
        }

        public async Task<IList<Project>> TestAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken)
        {
            if (projects.Count <= 0)
                _console.WriteEmphasizedLine("No projects found");

            foreach (var project in projects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                RunScriptsForTask(project, AppTask.Test, options);

                var projectAppDirectoryPaths = project.GetAppDirectoryPaths(_fileSystem);

                foreach (var path in projectAppDirectoryPaths)
                {
                    _queueService.Queue(project.Serial || options.NoParallel, async () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string appName = projectAppDirectoryPaths.Count > 1 ? $"{PathUtility.GetDirectoryShortName(path)} ({project.Name})" : project.Name;
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

                                    foreach (var requiredPath in requiredProject.GetAppDirectoryPaths(_fileSystem))
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

                        var testProcess = await _frameworkServiceProvider
                            .GetFrameworkService(project.Framework)
                            .TestProject(project, path, options, cancellationToken);

                        project.Processes.Add(testProcess);

                        if (project.Processes?.Count > 0)
                        {
                            var outputStatus = project.OutputStatus;
                            string outputMessage = $"Finished tests for {appName} with status {outputStatus}";

                            if (outputStatus == AppStatus.Success)
                                _console.WriteSuccessLine(outputMessage);
                            else
                                _console.WriteErrorLine(outputMessage);

                            await _projectManager.KillAllProcesses(project.RequiredProjects, options, cancellationToken);
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
                foreach (var path in project.GetAppDirectoryPaths(_fileSystem))
                {
                    _queueService.Queue(project.Serial || options.NoParallel, async () =>
                    {
                        _console.WriteEmphasizedLine($"Starting build for project {project.Name}");

                        var buildProcess = await _frameworkServiceProvider
                            .GetFrameworkService(project.Framework)
                            .BuildProject(project, path, options, cancellationToken);

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
                foreach (var path in project.GetAppDirectoryPaths(_fileSystem))
                {
                    _queueService.Queue(project.Serial || options.NoParallel, async () =>
                    {
                        if (!installedProjects.Contains(project.Name))
                        {
                            installedProjects.Add(project.Name);
                            await InternalInstallProject(project, path, options, cancellationToken);
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

                                    foreach (var requiredPath in requiredProject.GetAppDirectoryPaths(_fileSystem))
                                    {
                                        dependencyQueue.Queue(requiredProject.Serial || options.NoParallel, () =>
                                        {
                                            return InternalInstallProject(requiredProject, requiredPath, requiredOptions, cancellationToken);
                                        }, cancellationToken);
                                    }
                                }
                            }

                            await dependencyQueue.RunAllAsync(cancellationToken);
                        }
                    }, cancellationToken);
                }
            }

            await _queueService.RunAllAsync(cancellationToken);

            return projects;
        }

        private async Task<AppProcess> InternalInstallProject(Project project, string appDirectoryPath, RegiOptions options, CancellationToken cancellationToken)
        {
            _console.WriteEmphasizedLine($"Starting install for project {project.Name}");

            var process = await _frameworkServiceProvider
                .GetFrameworkService(project.Framework)
                .InstallProject(project, appDirectoryPath, options, cancellationToken);

            _console.WriteSuccessLine($"Finished installing project {project.Name}");

            project.Processes.Add(process);

            return process;
        }

        public async Task KillAsync(IList<Project> projects, RegiOptions options, CancellationToken cancellationToken)
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

                await _frameworkServiceProvider
                    .GetFrameworkService(framework)
                    .KillProcesses(options, cancellationToken);
            }

            _console.WriteSuccessLine("Finished killing processess successfuly", ConsoleLineStyle.LineBeforeAndAfter);
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
