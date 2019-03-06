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

            options.VariableList = new VariableList(projects);

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

            return projects;
        }

        private void StartProject(Project project, IList<Project> projects, CommandOptions options)
        {
            AppProcess process = null;
            if (project.Framework == ProjectFramework.Dotnet)
            {
                process = _dotnetService.RunProject(project, options);
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

            IList<Project> projects = config.Tests.FilterByOptions(options);

            options.VariableList = new VariableList(projects);

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
                                options.VariableList.AddProject(requiredProject);

                                _console.WriteLine(requiredProject.Name);
                                _console.WriteLine(requiredProject.Port);
                                _console.WriteLine(options.VariableList.Count);
                                StartProject(requiredProject, projects, options);
                            }
                        }
                    }

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        _console.WriteLine(project.File.FullName);
                        project.Process = _dotnetService.TestProject(project, options);
                    }
                    else if (project.Framework == ProjectFramework.Node)
                    {
                        project.Process = _nodeService.TestProject(project, options);
                    }

                    if (project.Process != null)
                    {
                        processes.Add(project.Process);
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
                    AppProcess process = null;

                    if (project.Framework == ProjectFramework.Dotnet)
                    {
                        process = _dotnetService.RestoreProject(project, options);
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
                            WritePropertyIfSpecified("Framework", app.Framework);
                            WritePropertyIfSpecified("Command", app.Command);
                            WritePropertyIfSpecified("Path", app.Path);
                            WritePropertyIfSpecified("Port", app.Port);

                            if (app.Requires != null && app.Requires.Count > 0)
                            {
                                WritePropertyIfSpecified("Requires", string.Join(", ", app.Requires));
                            }
                        }
                    }
                }

                void WritePropertyIfSpecified(string propertyName, object propertyValue)
                {
                    if (propertyValue == null || propertyValue is string propertyValueString && string.IsNullOrWhiteSpace(propertyValueString))
                        return;

                    _console.WriteIndentedLine($"{propertyName}: {propertyValue}", 2, ConsoleColor.DarkGreen);
                }
            }

            return output;
        }
    }
}
