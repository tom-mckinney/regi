using Regi.Extensions;
using System;
using System.Diagnostics;

namespace Regi.Models
{
    public class AppProcess : IDisposable
    {
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

        public int? Port { get; set; }

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

        public void Start()
        {
            if (Process != null)
            {
                Process.Start();
                Process.BeginErrorReadLine();

                if (Verbose)
                {
                    Process.BeginOutputReadLine();
                }
            }
        }

        public void WaitForExit()
        {
            if (Process != null)
            {
                Process.WaitForExit();
            }
        }

        public void Dispose()
        {
            if (KillOnExit && Process != null)
            {
                Process.KillAllOfType();
            }
        }
    }
}
