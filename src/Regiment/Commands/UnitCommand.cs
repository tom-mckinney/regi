using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regiment.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Regiment.Commands
{
    [Command("unit")]
    public class UnitCommand
    {
        private readonly IFileService _fileService;

        public UnitCommand(IFileService fileService)
        {
            _fileService = fileService; 
        }

        [Argument(0, Description = "Name of the project directory or file")]
        public string Name { get; set; }

        public int OnExecute()
        {
            List<FileInfo> projectFiles = _fileService.FindAllProjectFiles();

            if (projectFiles.Count > 0)
                return 0;

            //string currentDirectory = Directory.GetCurrentDirectory();
            //string fullPath = Path.GetRelativePath(currentDirectory, Name);
            //FileInfo projectFile = new FileInfo(fullPath);
            //if (projectFile.Exists)
            //{
            //    //DotNetExe.
            //    return 0;
            //}

            return 1;
        }
    }
}
