using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Regi
{
    public interface IProjectManager
    {
        IList<IProject> Projects { get; }

        CancellationTokenSource CancellationTokenSource { get; }

        IList<IProject> FilterAndTrackProjects(CommandOptions options, IServiceMesh config, Func<IServiceMesh, IEnumerable<IProject>> getTargetProjects);
        IList<IProject> FilterByOptions(IEnumerable<IProject> projects, CommandOptions options);
        Task KillAllProcesses(CommandOptions options, CancellationToken cancellationToken, bool logKillCount = false);
        Task KillAllProcesses(IEnumerable<IProject> projects, CommandOptions options, CancellationToken cancellationToken, bool logKillCount = false);
    }

    public class ProjectManager : IProjectManager
    {
        private readonly IConsole _console;
        private readonly ICleanupService _cleanupService;
        private readonly IProjectFilter _projectFilter;

        public ProjectManager(IConsole console, ICleanupService cleanupService, IProjectFilter projectFilter)
        {
            _console = console;
            _cleanupService = cleanupService;
            _projectFilter = projectFilter;
        }

        public IList<IProject> Projects { get; private set; } = new List<IProject>();

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        public IList<IProject> FilterAndTrackProjects(CommandOptions options, IServiceMesh config, Func<IServiceMesh, IEnumerable<IProject>> getTargetProjects)
        {
            Projects = FilterByOptions(getTargetProjects(config), options);

            LinkProjectRequirements(Projects, options, config);

            _console.CancelKeyPress += HandleCancelEvent;

            return Projects;
        }

        public IList<IProject> FilterByOptions(IEnumerable<IProject> projects, CommandOptions options)
        {
            // TODO: add a query builder pattern that combines filter expressions

            if (!string.IsNullOrWhiteSpace(options.Name))
            {
                projects = _projectFilter.FilterByName(projects, options.Name);
            }

            if (options.Labels?.Any() == true)
            {
                projects = _projectFilter.FilterByLabels(projects, options.Labels);
            }

            if (options.Exclude != null && options.Exclude.Any())
            {
                projects = _projectFilter.FilterByExclusions(projects, options.Exclude);
            }

            if (options.Roles?.Any() == true)
            {
                projects = _projectFilter.FilterByRoles(projects, options.Roles);
            }

            if (!options.IncludeOptional)
            {
                projects = _projectFilter.FilterByOptional(projects);
            }

            return projects.ToList();
        }

        public static void LinkProjectRequirements(IEnumerable<IProject> projects, CommandOptions options, IServiceMesh config)
        {
            foreach (var project in projects)
            {
                project.TryAddSource(options, config);

                foreach (var r in project.Requires)
                {
                    var requiredProject = config.Projects.FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));

                    if (requiredProject == null)
                        requiredProject = config.Services.FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));

                    if (requiredProject == null)
                        throw new Exception($"Could not find requirement \"{r}\" defined in project {project.Name}");

                    requiredProject.TryAddSource(options, config);

                    project.RequiredProjects.Add(requiredProject);
                }
            }
        }

        public Task KillAllProcesses(CommandOptions options, CancellationToken cancellationToken, bool logKillCount = false)
        {
            return KillAllProcesses(Projects, options, cancellationToken, logKillCount);
        }

        public async Task KillAllProcesses(IEnumerable<IProject> projects, CommandOptions options, CancellationToken cancellationToken, bool logKillCount = false)
        {
            int projectCount = projects.Count();
            if (projectCount > 0)
            {
                if (logKillCount)
                {
                    string projectPluralization = projectCount == 1 ? "project" : "projects";
                    _console.WriteDefaultLine($"Killing processes for {projects.Count()} {projectPluralization}");
                }

                foreach (var project in projects)
                {
                    if (project.Processes?.Count > 0)
                    {
                        foreach (var process in project.Processes)
                        {
                            if (process?.Process != null)
                            {
                                _cleanupService.KillProcessTree(process, options);
                            }
                        }
                    }
                }
            }

            await _cleanupService.ShutdownBuildServers(options, cancellationToken);
        }

        public ConsoleCancelEventHandler HandleCancelEvent => (o, e) =>
        {
            _console.WriteDefaultLine("Starting shutdown...");

            //foreach (var project in Projects)
            //{
            //    foreach (var process in project?.Processes)
            //    {
            //        process?.Process?.Kill(true);
            //    }
            //}
            //KillAllProcesses(options);

            CancellationTokenSource.Cancel();
        };
    }
}
