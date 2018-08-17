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
        IList<DotnetProcess> RunAsync(DirectoryInfo directory);
        IList<DotnetProcess> RunAsync(FileInfo startupFile);
    }

    public class RunnerService : IRunnerService
    {
        public IList<DotnetProcess> RunAsync(DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"Could not find directory: {directory.FullName}");
            }

            FileInfo startupFile = directory.GetFiles("startup.json").FirstOrDefault();

            if (startupFile == null)
            {
                throw new FileNotFoundException($"Could not find startup.json in directory: {directory.FullName}");
            }

            return RunAsync(startupFile);
        }

        public IList<DotnetProcess> RunAsync(FileInfo startupFile)
        {
            if (!startupFile.Exists)
            {
                throw new FileNotFoundException($"Startup file does not exist: {startupFile.FullName}");
            }

            StartupConfig config;

            using (StreamReader sr = new StreamReader(startupFile.OpenRead()))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = JsonSerializer.CreateDefault();

                config = serializer.Deserialize<StartupConfig>(reader);
            }

            if (config.Projects == null)
            {
                throw new JsonSerializationException($"Could not deserialize startup configuration from file: {startupFile.FullName}");
            }

            IList<DotnetProcess> processes = new List<DotnetProcess>();

            foreach (var project in config.Projects)
            {
                // Do the thing
            }

            return processes;
        }
    }
}
