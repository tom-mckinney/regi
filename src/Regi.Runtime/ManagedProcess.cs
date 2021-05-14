using Regi.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Runtime
{
    public class ManagedProcess : IManagedProcess
    {
        [DllImport("libc", SetLastError = true, EntryPoint = "kill")]
        private static extern int sys_kill(int pid, int sig);

        public ManagedProcess(string fileName, string arguments, DirectoryInfo workingDirectory, ILogSink logSink, IRuntimeInfo runtimeInfo)
            : this(Guid.NewGuid(), fileName, arguments, workingDirectory, logSink, runtimeInfo)
        {
        }

        public ManagedProcess(Guid id, string fileName, string arguments, DirectoryInfo workingDirectory, ILogSink logSink, IRuntimeInfo runtimeInfo)
        {
            Id = id;
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
            LogSink = logSink;
            RuntimeInfo = runtimeInfo;
        }

        public Guid Id { get; protected set; }

        public string FileName { get; protected set; }

        public string Arguments { get; protected set; }

        public DirectoryInfo WorkingDirectory { get; protected set; }

        public IRuntimeInfo RuntimeInfo { get; protected set; }

        internal Process Process { get; set; }

        internal ILogSink LogSink { get; set; }

        public async Task<IManagedProcessResult> StartAsync(CancellationToken cancellationToken)
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
                    CreateNoWindow = !RuntimeInfo.IsWindows,
                    WindowStyle = ProcessWindowStyle.Hidden
                },
                EnableRaisingEvents = true,
            };

            this.Process.OutputDataReceived += LogSink.OutputEventHandler;
            this.Process.ErrorDataReceived += LogSink.ErrorEventHandler;

            var processTcs = new TaskCompletionSource<IManagedProcessResult>();

            this.Process.Exited += (_, e) =>
            {
                this.Process.WaitForExit();

                processTcs.TrySetResult(new ManagedProcessResult(this.Process.ExitCode, LogSink));
            };

            this.Process.Start();
            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();

            var cancelledTsc = new TaskCompletionSource<object?>();
            await using var _ = cancellationToken.Register(() => cancelledTsc.TrySetResult(null));

            var result = await Task.WhenAny(processTcs.Task, cancelledTsc.Task);

            if (result == cancelledTsc.Task)
            {
                await KillAsync(cancellationToken);
            }

            var processResult = await processTcs.Task;
            return processResult;
        }

        public Task WaitForExitAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task KillAsync(CancellationToken cancellationToken)
        {
            if (this.Process == null)
            {
                return Task.CompletedTask;
            }

            if (!RuntimeInfo.IsWindows)
            {
                sys_kill(this.Process.Id, sig: 2);
            }
            else
            {
                if (!this.Process.CloseMainWindow())
                {
                    this.Process.Kill(true);
                }
            }

            if (!this.Process.HasExited)
            {
                // TODO: add unhappy path
            }

            return Task.CompletedTask;
        }
    }
}
