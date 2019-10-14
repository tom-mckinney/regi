using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Regi
{
    public interface IProjectManager
    {
        IList<Project> Projects { get; }

        CancellationTokenSource CancellationTokenSource { get; }

        IList<Project> FilterAndTrackProjects(RegiOptions options, StartupConfig config, Func<StartupConfig, IEnumerable<Project>> getTargetProjects);
        IList<Project> FilterByOptions(IEnumerable<Project> projects, RegiOptions options);
        Task KillAllProcesses(RegiOptions options, CancellationToken cancellationToken, bool logKillCount = false);
        Task KillAllProcesses(IEnumerable<Project> projects, RegiOptions options, CancellationToken cancellationToken, bool logKillCount = false);
    }

    public class ProjectManager : IProjectManager
    {
        private readonly IConsole _console;
        private readonly ICleanupService _cleanupService;

        public ProjectManager(IConsole console, ICleanupService cleanupService)
        {
            _console = console;
            _cleanupService = cleanupService;
        }

        public IList<Project> Projects { get; private set; } = new List<Project>();

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        public IList<Project> FilterAndTrackProjects(RegiOptions options, StartupConfig config, Func<StartupConfig, IEnumerable<Project>> getTargetProjects)
        {
            var targetProjects = getTargetProjects(config);

            Projects = FilterByOptions(targetProjects, options);

            LinkProjectRequirements(Projects, options, config);

            _console.CancelKeyPress += HandleCancelEvent;

            return Projects;
        }

        public IList<Project> FilterByOptions(IEnumerable<Project> projects, RegiOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Name))
            {
                projects = projects.Where(p => new Regex(options.Name, RegexOptions.IgnoreCase).IsMatch(p.Name));

            }

            if (options.Exclude != null && options.Exclude.Any())
            {
                projects = projects
                    .Where(p =>
                    {
                        foreach (var exclusion in options.Exclude)
                        {
                            if (new Regex(exclusion, RegexOptions.IgnoreCase).IsMatch(p.Name))
                                return false;
                        }

                        return true;
                    });
            }

            if (options.Type.HasValue)
            {
                projects = projects
                    .Where(p => p.Type == options.Type);
            }

            return projects.ToList();
        }

        public static void LinkProjectRequirements(IEnumerable<Project> projects, RegiOptions options, StartupConfig config)
        {
            foreach (var project in projects)
            {
                project.TryAddSource(options, config);

                foreach (var r in project.Requires)
                {
                    var requiredProject = config.Apps.FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));

                    if (requiredProject == null)
                        requiredProject = config.Services.FirstOrDefault(p => p.Name.Contains(r, StringComparison.InvariantCultureIgnoreCase));

                    if (requiredProject == null)
                        throw new Exception($"Could not find requirement \"{r}\" defined in project {project.Name}");

                    requiredProject.TryAddSource(options, config);

                    project.RequiredProjects.Add(requiredProject);
                }
            }
        }

        public Task KillAllProcesses(RegiOptions options, CancellationToken cancellationToken, bool logKillCount = false)
        {
            return KillAllProcesses(Projects, options, cancellationToken, logKillCount);
        }

        public async Task KillAllProcesses(IEnumerable<Project> projects, RegiOptions options, CancellationToken cancellationToken, bool logKillCount = false)
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
