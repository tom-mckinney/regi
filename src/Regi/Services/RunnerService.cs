﻿using McMaster.Extensions.CommandLineUtils;
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

            foreach (var project in projects)
            {
                _parallelService.Queue(() =>
                {
                    StartProject(project, null, options);
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

            WaitOnPorts(projects);

            return projects;
        }

        private void StartProject(Project project, IList<Project> projects, CommandOptions options)
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

            foreach (var project in projects)
            {
                _parallelService.Queue(() =>
                {
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
                                if (project.Port.HasValue)
                                    requiredProjectsWithPorts.Add(project.Port.Value, project);

                                StartProject(requiredProject, projects, options);
                            }
                        }

                        WaitOnPorts(requiredProjectsWithPorts);
                    }

                    _console.WriteEmphasizedLine($"Starting tests for project {project.Name} ({project.File.DirectoryName})");

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
                        processes.Add(project.Process);

                        _console.WriteEmphasizedLine($"Finished tests for project {project.Name} ({project.File.DirectoryName})");
                    }
                });
            }

            _parallelService.RunInParallel();

            return projects;
        }

        public IList<Project> Install(CommandOptions options)
        {
            StartupConfig config = GetStartupConfig();

            IList<Project> projects = config.Apps.Concat(config.Tests).FilterByOptions(options);

            foreach (var project in projects)
            {
                _parallelService.Queue(() =>
                {
                    _console.WriteEmphasizedLine($"Starting install for project {project.Name}");

                    AppProcess process = null;

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        process = _dotnetService.InstallProject(project, options);
                    }
                    else if (project.Framework == ProjectFramework.Node)
                    {
                        process = _nodeService.InstallProject(project, new CommandOptions { Verbose = true });
                    }

                    if (process != null)
                    {
                        project.Process = process;
                    }

                });
            }

            _parallelService.RunInParallel();

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

        private void WaitOnPorts(IList<Project> projects)
        {
            IDictionary<int, Project> projectsWithPorts = projects
                .Where(p => p.Port.HasValue)
                .ToDictionary(p => p.Port.Value);

            WaitOnPorts(projectsWithPorts);
        }

        private void WaitOnPorts(IDictionary<int, Project> projects)
        {
            string projectPluralization = projects.Count > 1 ? "projects" : "project";
            _console.WriteEmphasizedLine($"Waiting for {projectPluralization} to start: {string.Join(", ", projects.Select(p => $"{p.Value.Name} ({p.Key})"))}");

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            while (projects.Count > 0)
            {
                IPEndPoint[] listeningConnections = ipGlobalProperties.GetActiveTcpListeners();

                foreach (var connection in listeningConnections)
                {
                    if (projects.TryGetValue(connection.Port, out Project p))
                    {
                        _console.WriteEmphasizedLine($"{p.Name} is now listening on port {connection.Port}");

                        projects.Remove(connection.Port);
                    }
                }
            }
        }
    }
}
