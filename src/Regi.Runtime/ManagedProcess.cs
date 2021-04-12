using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Runtime
{
    public class ManagedProcess : IManagedProcess
    {
        private readonly ILogSink _logSink;

        public ManagedProcess(string fileName, string arguments, DirectoryInfo workingDirectory, ILogSink logSink)
        {
            Id = Guid.NewGuid();
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
            _logSink = logSink;
        }

        public Guid Id { get; protected set; }

        public string FileName { get; protected set; }

        public string Arguments { get; protected set; }

        public DirectoryInfo WorkingDirectory { get; protected set; }

        internal Process Process { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = FileName,
                    Arguments = Arguments,
                    WorkingDirectory = WorkingDirectory.FullName,
                    RedirectStandardInput = true, // required
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true,
            };

            this.Process.OutputDataReceived += _logSink.OutputHandler;
            this.Process.ErrorDataReceived += _logSink.ErrorHandler;

            this.Process.Start();
            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();

            return Task.CompletedTask;
        }

        public Task WaitForExitAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task KillAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
