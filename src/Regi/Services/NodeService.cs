using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Regi.Services
{
    public interface INodeService
    {
        AppProcess StartProject(FileInfo projectFile, bool verbose = false, int? port = null);
    }

    public class NodeService : CLIBase, INodeService
    {
        private readonly IConsole _console;
        private readonly string _npmPath;

        public NodeService(IConsole console) : base(console)
        {
            _console = console;

            _npmPath = NpmExe.FullPathOrDefault();

            if (string.IsNullOrWhiteSpace(_npmPath))
            {
                throw new FileNotFoundException("Could not find path for NPM CLI");
            }
        }

        public AppProcess StartProject(FileInfo projectFile, bool verbose = false, int? port = null)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _npmPath,
                    Arguments = "start",
                    WorkingDirectory = projectFile.DirectoryName,
                    RedirectStandardOutput = verbose,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };

            if (port.HasValue)
            {
                process.StartInfo.EnvironmentVariables.Add("PORT", port.Value.ToString());
            }

            AppProcess output = new AppProcess(process, AppTask.Run, AppStatus.Running, port);

            process.ErrorDataReceived += DefaultErrorDataReceived(output);
            process.Exited += DefaultExited(output);
            if (verbose)
            {
                process.OutputDataReceived += DefaultOutputDataRecieved();
            }

            process.Start();

            process.BeginErrorReadLine();
            if (verbose)
            {
                process.BeginOutputReadLine();
            }

            return output;
        }
    }
}
