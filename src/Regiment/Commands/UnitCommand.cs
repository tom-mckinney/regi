using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regiment.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Regiment.Commands
{
    [Command("unit")]
    public class UnitCommand
    {
        private readonly IFileService _fileService;
        private readonly IConsole _console;

        public UnitCommand(IFileService fileService, IConsole console)
        {
            _fileService = fileService;
            _console = console;
        }

        [Argument(0, Description = "Name of the project directory or file")]
        public string Name { get; set; }

        public int OnExecute()
        {
            List<FileInfo> projectFiles = _fileService.FindAllProjectFiles();

            if (projectFiles.Count <= 0)
            {
                return 1;
            }

            foreach(var file in projectFiles)
            {
                ProcessStartInfo unitTestInfo = new ProcessStartInfo()
                {
                    FileName = DotNetExe.FullPath,
                    Arguments = "test",
                    WorkingDirectory = file.DirectoryName,
                    RedirectStandardOutput = true
                };

                var unitTest = Process.Start(unitTestInfo);

                using (StreamReader output = unitTest.StandardOutput)
                {
                    _console.Write(output.ReadToEnd());
                }
            }

            //string currentDirectory = Directory.GetCurrentDirectory();
            //string fullPath = Path.GetRelativePath(currentDirectory, Name);
            //FileInfo projectFile = new FileInfo(fullPath);
            //if (projectFile.Exists)
            //{
            //    //DotNetExe.
            //    return 0;
            //}

            return 0;
        }
    }
}
