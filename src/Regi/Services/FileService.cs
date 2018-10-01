using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Regi.Services
{
    public interface IFileService
    {
        FileInfo CreateConfigFile();
        List<FileInfo> FindAllProjectFiles();
        string FindFileOrDirectory(string fileName);
    }

    public class FileService : IFileService
    {
        private readonly CommandLineContext _context;
        private readonly IConsole _console;

        public FileService(CommandLineContext context, IConsole console)
        {
            _context = context;
            _console = console;
        }

        public FileInfo CreateConfigFile()
        {
            string configFilePath = Path.Combine(DirectoryUtility.TargetDirectoryPath, "regi.json");

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
            DirectoryInfo currentDirectory = new DirectoryInfo(_context.WorkingDirectory);

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
