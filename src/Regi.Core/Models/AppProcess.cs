using Regi.Abstractions;
using Regi.Abstractions.Options;
using Regi.Extensions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Models
{
    public class AppProcess : IAppProcess
    {
        private static readonly object _lock = new object();

        public AppProcess(Process process, AppTask task, AppStatus status, int? port = null)
        {
            Process = process;
            Task = task;
            Status = status;
            Port = port;
        }

        public AppTask Task { get; set; }

        public AppStatus Status { get; set; }

        public virtual Process Process { get; protected set; }

        public int ProcessId { get; protected set; }
        public string ProcessName { get; protected set; }

        public int? Port { get; set; }

        public string Path { get; set; }

        public DateTimeOffset? StartTime
        {
            get
            {
                try
                {
                    return Process?.StartTime;
                }
                catch
                {
                    return null;
                }
            }
        }

        public DateTimeOffset? EndTime { get; set; }

        public bool KillOnExit { get; set; } = true;

        public bool Verbose { get; set; } = false;

        public bool RawOutput { get; internal set; }

        public bool ErrorDataHandled { get; internal set; } = false;
        public bool OutputDataHandled { get; internal set; } = false;


        public void Start()
        {
            if (Process == null)
                throw new InvalidOperationException("Process cannot be null when starting");

            lock (_lock)
            {
                Process.Start();

                if (!RawOutput)
                {
                    Process.BeginErrorReadLine();
                    Process.BeginOutputReadLine();
                }

                ProcessId = Process.Id;
                ProcessName = Process.ProcessName;
            }
        }

        public Action<int> OnKill { get; set; }

        public Task WaitForExitAsync(CancellationToken cancellationToken)
        {
            return Process?.WaitForExitAsync(cancellationToken);
        }

        public void Kill(OptionsBase options = null)
        {
            Kill(Constants.DefaultTimeout, options);
        }

        public void Kill(TimeSpan timeout, OptionsBase options = null)
        {
            OnKill?.Invoke(ProcessId);

            if (KillOnExit)
            {
                try
                {
                    Process?.Kill();
                    Process?.WaitForExit(timeout.Milliseconds);
                }
                catch (Exception e)
                {
                    if (options?.Verbose == true)
                    {
                        Console.WriteLine($"Exception was thrown while exiting process with PID {ProcessId}. Details: {e.Message}"); // TODO: use ILogger
                        //console?.WriteErrorLine($"Exception was thrown while exiting process with PID {ProcessId}. Details: {e.Message}");
                    }
                }
            }
        }
    }
}
