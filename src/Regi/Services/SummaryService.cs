using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regi.Services
{
    public interface ISummaryService
    {
        OutputSummary PrintTestSummary(IList<Project> projects, TimeSpan timespan);
    }

    public class SummaryService : ISummaryService
    {
        private readonly IConsole _console;

        public SummaryService(IConsole console)
        {
            _console = console;
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
                switch (project.Process.Status)
                {
                    case AppStatus.Failure:
                        PrintBadge("FAIL", ConsoleColor.Red);
                        failCount++;
                        break;
                    case AppStatus.Success:
                        PrintBadge("PASS", ConsoleColor.Green);
                        successCount++;
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

        private void PrintBadge(string status, ConsoleColor backgroundColor)
        {
            _console.BackgroundColor = backgroundColor;
            _console.ForegroundColor = ConsoleColor.Black;

            _console.Write($" {status} ");

            _console.ResetColor();
        }
    }
}
