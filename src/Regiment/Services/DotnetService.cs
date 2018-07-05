using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Regiment.Services
{
    public interface IDotnetService
    {
        void TestProject(FileInfo projectFile, bool verbose);
    }

    public class DotnetService : IDotnetService
    {
        private IConsole _console;

        public DotnetService(IConsole console)
        {
            _console = console;
        }

        public void TestProject(FileInfo projectFile, bool verbose = false)
        {
            ProcessStartInfo unitTestInfo = new ProcessStartInfo()
            {
                FileName = DotNetExe.FullPath,
                Arguments = "test",
                WorkingDirectory = projectFile.DirectoryName,
                RedirectStandardOutput = verbose,
                RedirectStandardError = true
            };

            var unitTest = Process.Start(unitTestInfo);

            if (verbose)
            {
                using (StreamReader stdOutput = unitTest.StandardOutput)
                {
                    _console.Write(stdOutput.ReadToEnd());
                }
            }

            using (StreamReader stdError = unitTest.StandardError)
            {
                Span<char> buffer = new Span<char>();
                while (!stdError.EndOfStream)
                {
                    stdError.Read(buffer);
                    _console.Write(buffer);
                    Console.Write(buffer);
                }
                _console.Write(await stdError.ReadToEndAsync());
            }
        }
    }
}
