using Regi.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

        public DateTimeOffset? Start
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

        public DateTimeOffset? End { get; set; }

        public void Dispose()
        {
            Process.KillTree(TimeSpan.FromSeconds(2));
        }
    }
}
