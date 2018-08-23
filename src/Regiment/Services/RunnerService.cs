using Newtonsoft.Json;
using Regiment.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Regiment.Services
{
    public interface IRunnerService
    {
        StartupConfig GetStartupConfig(string path);
        IList<DotnetProcess> RunAsync(string directoryName);
        IList<DotnetProcess> TestAsync(string directoryName);
    }

    public class RunnerService : IRunnerService
    {
        private readonly IDotnetService _dotnetService;

        public RunnerService(IDotnetService dotnetService)
        {
            _dotnetService = dotnetService;
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
                        processes.Add(_dotnetService.RunProject(projectFile));
                    }
                }
            }

            return processes;
        }

        public IList<DotnetProcess> TestAsync(string directoryName)
        {
            StartupConfig config = GetStartupConfig(directoryName);

            IList<DotnetProcess> processes = new List<DotnetProcess>();

            foreach (var project in config.Tests)
            {
                if (project.Type == ProjectType.Test)
                {
                    string absolutePath = Path.GetFullPath(project.Path, directoryName);
                    FileInfo projectFile = new FileInfo(absolutePath);

                    if (projectFile.Exists)
                    {
                        processes.Add(_dotnetService.TestProject(projectFile));
                    }
                }
            }

            return processes;
        }
    }
}
