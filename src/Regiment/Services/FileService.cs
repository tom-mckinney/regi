using McMaster.Extensions.CommandLineUtils.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Regiment.Services
{
    public interface IFileService
    {
        List<FileInfo> FindAllProjectFiles();
        string FindFileOrDirectory(string fileName);
    }

    public class FileService : IFileService
    {
        private readonly CommandLineContext _context;

        public FileService(CommandLineContext context)
        {
            _context = context;
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
