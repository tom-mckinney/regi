using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Regi.Services
{
    public interface IFileSystem
    {
        string WorkingDirectory { get; set; }

        string GetDirectoryPath(string path, bool throwIfNotFound = true, string targetObj = "project");
        FileInfo CreateConfigFile();
        List<FileInfo> FindAllProjectFiles();
        string FindFileOrDirectory(string fileName);
    }

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

        public FileInfo CreateConfigFile()
        {
            string configFilePath = Path.Combine(WorkingDirectory, "regi.json");

            if (File.Exists(configFilePath))
            {
                throw new InvalidOperationException($"There is already a {nameof(Regi)} configuration file at path: {configFilePath}");
            }

            _console.WriteEmphasizedLine($"Creating config file: {configFilePath}");

            FileInfo configFile = new FileInfo(configFilePath);
            using (var stream = configFile.Create())
            {
                byte[] content = Encoding.UTF8.GetBytes(@"{
  ""apps"": [],
  ""tests"": [],
  ""services"": []
}");
                stream.Write(content, 0, content.Length);
            }

            _console.WriteEmphasizedLine($"Success!");

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
    }
}
