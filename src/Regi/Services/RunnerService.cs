using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Regi.Services
{
    public interface IRunnerService
    {
        StartupConfig GetStartupConfig(string path);
        IList<DotnetProcess> RunAsync(string directoryName);
        IList<DotnetProcess> TestAsync(string directoryName, ProjectType? type = null);
    }

    public class RunnerService : IRunnerService
    {
        private readonly IDotnetService _dotnetService;
        private readonly IConsole _console;

        public RunnerService(IDotnetService dotnetService, IConsole console)
        {
            _dotnetService = dotnetService;
            _console = console;
        }

        public StartupConfig GetStartupConfig(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"Could not find directory: {directory.FullName}");
            }

            FileInfo startupFile = directory.GetFiles("startup.json").FirstOrDefault();

            if (startupFile == null || !startupFile.Exists)
            {
                throw new FileNotFoundException($"Could not find startup.json in directory: {directory.FullName}");
            }

            using (StreamReader sr = new StreamReader(startupFile.OpenRead()))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = JsonSerializer.CreateDefault();
                serializer.MissingMemberHandling = MissingMemberHandling.Error;

                return serializer.Deserialize<StartupConfig>(reader);
            }
        }

        public IList<DotnetProcess> RunAsync(string directoryName)
        {
            StartupConfig config = GetStartupConfig(directoryName);

            IList<DotnetProcess> processes = new List<DotnetProcess>();


            foreach (var project in config.Apps)
            {
                if (project.Type == ProjectType.Web)
                {
                    string absolutePath = Path.GetFullPath(project.Path, directoryName);
                    FileInfo projectFile = new FileInfo(absolutePath);

                    if (projectFile.Exists)
                    {
                        processes.Add(_dotnetService.RunProject(projectFile, false, project.Port));
                    }
                }
            }

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var process in processes)
                {
                    process.Process.KillTree(TimeSpan.FromSeconds(10));
                }
            };

            return processes;
        }

        public IList<DotnetProcess> TestAsync(string directoryName, ProjectType? type = null)
        {
            StartupConfig config = GetStartupConfig(directoryName);

            IList<DotnetProcess> processes = new List<DotnetProcess>();

            Console.CancelKeyPress += (o, e) =>
            {
                foreach (var process in processes)
                {
                    process.Process.KillTree(TimeSpan.FromSeconds(10));
                }
            };

            foreach (var project in config.Tests)
            {
                if (!type.HasValue || project.Type == type)
                {
                    string absolutePath = Path.GetFullPath(project.Path, directoryName);
                    FileInfo projectFile = new FileInfo(absolutePath);

                    if (projectFile.Exists)
                    {
                        _console.WriteLine(projectFile.FullName);
                        processes.Add(_dotnetService.TestProject(projectFile));
                    }
                }
            }

            return processes;
        }
    }
}
