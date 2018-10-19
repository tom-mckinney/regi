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

        public void Dispose()
        {
            if (Process != null && !Process.HasExited)
            {
                Process.KillAllOfType();
            }
        }
    }
}
