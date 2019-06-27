using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Regi
{
    public interface IProjectManager
    {
        IList<Project> Projects { get; }

        IList<Project> FilterAndTrackProjects(RegiOptions options, params IEnumerable<Project>[] projectCollections);

        IList<Project> FilterByOptions(IEnumerable<Project> projects, RegiOptions options);
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

        public IList<Project> FilterAndTrackProjects(RegiOptions options, params IEnumerable<Project>[] projectCollections)
        {
            var targetProjects = projectCollections.Aggregate((acc, list) => acc.Concat(list));

            Projects = FilterByOptions(targetProjects, options);

            _console.CancelKeyPress += HandleCancelEvent(options);

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

        public ConsoleCancelEventHandler HandleCancelEvent(RegiOptions options) => (o, e) =>
        {
            _console.WriteEmphasizedLine("Shutting down processes");

            if (Projects?.Count > 0)
            {
                foreach (var project in Projects)
                {
                    if (project.Process != null)
                    {
                        project.Process.Kill(_console);
                    }
                }
            }

            _cleanupService.ShutdownBuildServers(options);
        };
    }
}
