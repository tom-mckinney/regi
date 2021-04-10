using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Regi.Services
{

    public class FileSystem : IFileSystem
    {
        private readonly IConsole _console;

        public FileSystem(IConsole console)
        {
            _console = console;
        }

        public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();

        public string GetDirectoryPath(string path, bool throwIfNotFound = true, string targetObj = "project")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (throwIfNotFound)
                {
                    throw new ArgumentException("Path cannot be null", nameof(path));
                }
                else
                {
                    return null;
                }
            }

            string absolutePath = Path.GetFullPath(path, WorkingDirectory);

            if (Directory.Exists(absolutePath))
            {
                return absolutePath;
            }
            else if (File.Exists(absolutePath))
            {
                return Path.GetDirectoryName(absolutePath);
            }
            else if (throwIfNotFound)
            {
                throw new DirectoryNotFoundException($"Could not find {targetObj}, {absolutePath}");
            }

            return null;
        }

        public string GetRelativePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            string relativePath = Path.GetRelativePath(WorkingDirectory, path).Replace('\\', '/');

            if (relativePath.Length > 2)
            {
                if (relativePath[0] == '.' && (relativePath[1] == '.' || relativePath[1] == '/'))
                {
                    return relativePath;
                }
                else
                {
                    return $"./{relativePath}";
                }
            }
            else if (relativePath[0] == '.')
            {
                return "./";
            }
            else
            {
                throw new InvalidOperationException($"Unexpected relative path: {relativePath}");
            }
        }

        public async ValueTask<FileInfo> CreateConfigFileAsync(IServiceMesh config)
        {
            string configFilePath = Path.Combine(WorkingDirectory, "regi.json");

            if (File.Exists(configFilePath))
            {
                throw new RegiException($"There is already a {nameof(Regi)} configuration file at path: {configFilePath}");
            }

            _console.WriteEmphasizedLine($"Creating config file: {configFilePath}");

            FileInfo configFile = new FileInfo(configFilePath);

            using var fileStream = configFile.Create();
            await JsonSerializer.SerializeAsync(fileStream, config, typeof(ServiceMesh), Constants.DefaultSerializerOptions);

            _console.WriteEmphasizedLine($"Job's done.");

            return configFile;
        }

        public List<FileInfo> FindAllProjectFiles()
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(WorkingDirectory);

            if (!currentDirectory.Exists)
            {
                throw new Exception("Working directory doesn't exist");
            }

            return currentDirectory.GetFiles("*.csproj", SearchOption.AllDirectories).ToList();
        }

        public string FindFileOrDirectory(string fileName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DirectoryInfo> GetChildDirectories(DirectoryInfo directory)
        {
            if (directory?.Exists != true)
            {
                throw new ArgumentException($"Directory must exist. Recieved: {directory?.FullName}", nameof(directory));
            }

            return directory.GetDirectories();
        }

        public IFileSystemDictionary GetAllChildren(DirectoryInfo directory)
        {
            if (directory?.Exists != true)
            {
                throw new ArgumentException($"Directory must exist. Recieved: {directory?.FullName}", nameof(directory));
            }

            return new FileSystemDictionary(directory);
        }
    }
}
