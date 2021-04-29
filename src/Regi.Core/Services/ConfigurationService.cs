using Regi.Abstractions;
using Regi.Extensions;
using Regi.Models;
using Regi.Runtime;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IConfigurationService
    {
        Task<IServiceMesh> GetConfigurationAsync(CommandOptions options);
        ValueTask<IServiceMesh> CreateConfigurationAsync(IEnumerable<IProject> projects, CommandOptions options);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly IFileSystem _fileSystem;

        public ConfigurationService(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public ValueTask<IServiceMesh> CreateConfigurationAsync(IEnumerable<IProject> projects, CommandOptions options)
        {
            var config = new ServiceMesh
            {
                Projects = projects.ToList()
            };

            return new ValueTask<IServiceMesh>(config);
        }

        public async Task<IServiceMesh> GetConfigurationAsync(CommandOptions options)
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

                var config = await JsonSerializer.DeserializeAsync<ServiceMesh<Project, ServiceOmnibus>>(stream, Constants.DefaultSerializerOptions);

                config.Path = startupFile.FullName;

                return (ServiceMesh)config;
            }
            catch (Exception e)
            {
                string relativePath = Path.GetRelativePath(_fileSystem.WorkingDirectory, startupFile.FullName);

                throw new RegiException($"Configuration file was not properly formatted: {relativePath}{Environment.NewLine}{ExceptionUtility.GetMessage(e)}", e);
            }
        }
    }
}
