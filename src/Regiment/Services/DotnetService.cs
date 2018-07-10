using McMaster.Extensions.CommandLineUtils;
using Regiment.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Regiment.Services
{
    public interface IDotnetService
    {
        DotnetProcess TestProject(FileInfo projectFile, bool verbose);
    }

    public class DotnetService : IDotnetService
    {
        private readonly StringBuilder _stringBuilder;
        private IConsole _console;

        public DotnetService(IConsole console)
        {
            _stringBuilder = new StringBuilder();
            _console = console;
        }

        public DotnetProcess TestProject(FileInfo projectFile, bool verbose = false)
        {
            ProcessStartInfo unitTestInfo = new ProcessStartInfo()
            {
                FileName = DotNetExe.FullPath,
                Arguments = "test",
                WorkingDirectory = projectFile.DirectoryName,
                RedirectStandardOutput = verbose,
                RedirectStandardError = true
            };

            DotnetProcess unitTest = new DotnetProcess(Process.Start(unitTestInfo), DotnetTask.Test);

            if (verbose)
            {
                using (StreamReader stdOutput = unitTest.Process.StandardOutput)
                {
                    _console.Write(stdOutput.ReadToEnd());

                    //Span<char> buffer = new Span<char>();
                    //while (!stdOutput.EndOfStream)
                    //{
                    //    stdOutput.Read(buffer);
                    //    _console.Write(buffer.ToArray());

                    //    if (buffer.Length == 0)
                    //    {
                    //        break;
                    //    }
                    //}
                    //_console.Write(stdOutput.ReadToEnd());
                }
            }

            using (StreamReader stdError = unitTest.Process.StandardError)
            {
                string errors = stdError.ReadToEnd();
                _console.Write(errors);

                unitTest.Result = !string.IsNullOrWhiteSpace(errors) ? DotnetResult.Failure : DotnetResult.Success;

                //stdError.ReadToEnd();
                //Span<char> buffer = new Span<char>();
                //while (!stdError.EndOfStream)
                //{
                //    stdError.Read(buffer);
                //    _console.Write(buffer.ToArray());

                //    if (buffer.Length == 0)
                //    {
                //        break;
                //    }
                //}
            }

            unitTest.End = DateTimeOffset.Now;

            return unitTest;
        }
    }
}
