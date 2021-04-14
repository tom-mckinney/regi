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
        public ManagedProcess(string fileName, string arguments, DirectoryInfo workingDirectory, ILogSink logSink)
            : this(Guid.NewGuid(), fileName, arguments, workingDirectory, logSink)
        {
        }

        public ManagedProcess(Guid id, string fileName, string arguments, DirectoryInfo workingDirectory, ILogSink logSink)
        {
            Id = id;
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
            LogSink = logSink;
        }

        public Guid Id { get; protected set; }

        public string FileName { get; protected set; }

        public string Arguments { get; protected set; }

        public DirectoryInfo WorkingDirectory { get; protected set; }

        internal Process Process { get; set; }

        internal ILogSink LogSink { get; set; }

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

            this.Process.OutputDataReceived += LogSink.OutputEventHandler;
            this.Process.ErrorDataReceived += LogSink.ErrorEventHandler;

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
