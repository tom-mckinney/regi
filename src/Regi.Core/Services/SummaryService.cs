using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regi.Services
{
    public interface ISummaryService
    {
        OutputSummary PrintDomainSummary(StartupConfig config, RegiOptions options);
        OutputSummary PrintTestSummary(IList<Project> projects, TimeSpan timespan);
    }

    public class SummaryService : ISummaryService
    {
        private readonly IProjectManager _projectManager;
        private readonly IFileSystem _fileSystemService;
        private readonly IConsole _console;

        public SummaryService(IProjectManager projectManager, IFileSystem fileSystem, IConsole console)
        {
            _projectManager = projectManager;
            _fileSystemService = fileSystem;
            _console = console;
        }

        public OutputSummary PrintDomainSummary(StartupConfig config, RegiOptions options)
        {
            options.IncludeOptional = true; // Always include optional projects that match criteria

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

                        _console.Write($"  {app.Name}");

                        if (app.Optional)
                        {
                            _console.ForegroundColor = ConsoleColor.DarkGray;
                            _console.Write(" (Optional)");
                            _console.ResetColor();
                        }

                        _console.WriteLine();

                        if (options.Verbose)
                        {
                            _console.WritePropertyIfSpecified("Framework", app.Framework);
                            _console.WritePropertyIfSpecified("Type", app.Type);
                            _console.WritePropertyIfSpecified("Paths", app.GetAppDirectoryPaths(_fileSystemService), true, 2);
                            _console.WritePropertyIfSpecified("Port", app.Port);
                            _console.WritePropertyIfSpecified("Commands", app.Commands);
                            _console.WritePropertyIfSpecified("Requires", app.Requires);
                            _console.WritePropertyIfSpecified("Arguments", app.Arguments);
                            _console.WritePropertyIfSpecified("Environment", app.Environment);
                            _console.WritePropertyIfSpecified("Serial", app.Serial);
                            _console.WritePropertyIfSpecified("Raw Output", app.RawOutput);
                        }
                    }
                }
            }

            return output;
        }

        public OutputSummary PrintTestSummary(IList<Project> projects, TimeSpan timespan)
        {
            int failCount = 0;
            int successCount = 0;
            int runningCount = 0;
            int unknownCount = 0;

            _console.WriteLine(); // Padding

            foreach (var project in projects)
            {
                switch (project.OutputStatus)
                {
                    case AppStatus.Success:
                        PrintBadge("PASS", ConsoleColor.Green);
                        successCount++;
                        break;
                    case AppStatus.Failure:
                        PrintBadge("FAIL", ConsoleColor.Red);
                        failCount++;
                        break;
                    case AppStatus.Running:
                        PrintBadge("RUNNING", ConsoleColor.DarkYellow);
                        runningCount++;
                        break;
                    case AppStatus.Unknown:
                        PrintBadge("UNKNOWN", ConsoleColor.Gray);
                        unknownCount++;
                        break;
                    default:
                        throw new InvalidOperationException("Recieved project with invalid status.");
                }

                _console.WriteLine($" {project.Name}");

                if (project.Processes?.Count > 1)
                {
                    foreach (var process in project.Processes)
                    {
                        _console.Write(' ');

                        switch (process.Status)
                        {
                            case AppStatus.Failure:
                                PrintBadge("FAIL", ConsoleColor.Red, 0);
                                break;
                            case AppStatus.Success:
                                PrintBadge("PASS", ConsoleColor.Green, 0);
                                break;
                            case AppStatus.Running:
                                PrintBadge("RUNNING", ConsoleColor.DarkYellow, 0);
                                break;
                            case AppStatus.Unknown:
                                PrintBadge("UNKNOWN", ConsoleColor.Gray, 0);
                                break;
                            default:
                                throw new InvalidOperationException("Recieved project with invalid status.");
                        }

                        _console.WriteLine($" {PathUtility.GetDirectoryShortName(process.Path)}");
                    }                    
                }
            }

            List<(string Message, ConsoleColor Color)> outputDescriptors = new List<(string Message, ConsoleColor Color)>();

            if (failCount > 0)
            {
                outputDescriptors.Add(($"{failCount} failed", ConsoleColor.Red));
            }
            if (successCount > 0)
            {
                outputDescriptors.Add(($"{successCount} succeeded", ConsoleColor.Green));
            }
            if (runningCount > 0)
            {
                outputDescriptors.Add(($"{runningCount} running", ConsoleColor.DarkYellow));
            }
            if (unknownCount > 0)
            {
                outputDescriptors.Add(($"{unknownCount} unknown", ConsoleColor.Gray));
            }

            outputDescriptors.Add(($"{projects.Count} total", ConsoleColor.White));


            _console.WriteLine(); // Padding
            _console.Write("Test projects: ");

            for (int i = 0; i < outputDescriptors.Count; i++)
            {
                if (i != 0)
                {
                    _console.ResetColor();
                    _console.Write(", ");
                }

                var (Message, Color) = outputDescriptors[i];

                _console.ForegroundColor = Color;
                _console.Write(Message);
                _console.ResetColor();
            }

            _console.WriteLine();
            PrintElapsedTime(timespan);

            return new OutputSummary
            {
                SuccessCount = successCount,
                FailCount = failCount,
                UnknownCount = unknownCount
            };
        }

        private void PrintElapsedTime(TimeSpan elapsed)
        {
            _console.WriteDefaultLine($"Elapsed time: {elapsed.ToHumanFriendlyString()}", ConsoleLineStyle.LineAfter);
        }

        private void PrintBadge(string status, ConsoleColor backgroundColor, int? indentCount = null)
        {
            if (indentCount.HasValue)
            {
                _console.Write(new string(' ', indentCount.Value * 2));
            }

            _console.BackgroundColor = backgroundColor;
            _console.ForegroundColor = ConsoleColor.Black;

            _console.Write($" {status} ");

            _console.ResetColor();
        }
    }
}
