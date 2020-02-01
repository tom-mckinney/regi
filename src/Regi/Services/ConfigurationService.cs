using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IConfigurationService
    {
        Task<StartupConfig> GetConfigurationAsync(RegiOptions options);
        ValueTask<StartupConfig> CreateConfigurationAsync(IEnumerable<Project> projects, RegiOptions options);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IFileSystem _fileSystem;

        public ConfigurationService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public ValueTask<StartupConfig> CreateConfigurationAsync(IEnumerable<Project> projects, RegiOptions options)
        {
            var config = new StartupConfig();

            foreach (var project in projects)
            {
                if (project.Type == ProjectType.Unit || project.Type == ProjectType.Integration)
                {
                    config.Tests.Add(project);
                }
                else
                {
                    config.Apps.Add(project);
                }
            }

            return new ValueTask<StartupConfig>(config);
        }

        public async Task<StartupConfig> GetConfigurationAsync(RegiOptions options)
        {
            DirectoryInfo directory;

            if (!string.IsNullOrWhiteSpace(options?.ConfigurationPath))
            {
                string configDirectory = _fileSystem.GetDirectoryPath(options.ConfigurationPath, true, "directory");
                _fileSystem.WorkingDirectory = configDirectory;
                directory = new DirectoryInfo(configDirectory);
            }
            else
            {
                directory = new DirectoryInfo(_fileSystem.WorkingDirectory);
            }

            if (directory?.Exists == false)
            {
                throw new DirectoryNotFoundException($"Could not find directory: {directory.FullName}");
            }

            FileInfo startupFile = directory.GetOneOfFiles("regi.json", "startup.json");

            if (startupFile == null || !startupFile.Exists)
            {
                throw new RegiException($"Could not find regi.json or startup.json in directory: {directory.FullName}");
            }

            using var stream = startupFile.OpenRead();

            try
            {
                
                StartupConfig config = await JsonSerializer.DeserializeAsync<StartupConfig>(stream, Constants.DefaultSerializerOptions);

                config.Path = startupFile.FullName;

                return config;
            }
            catch (Exception e)
            {
                throw new RegiException($"Configuration file was not properly formatted: {startupFile.FullName}{Environment.NewLine}{e.Message}", e);
            }
        }
    }
}
