using Regi.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regi.Models
{
    public enum DotnetTask
    {
        Test = 1,
        Run,
    }

    public enum DotnetStatus
    {
        Unknown,
        Failure = 1,
        Success,
        Running
    }

    public class DotnetProcess : IDisposable
    {
        public DotnetProcess(Process process, DotnetTask task, DotnetStatus status)
        {
            Process = process;
            Task = task;
            Status = status;
        }

        public DotnetTask Task { get; set; }

        public DotnetStatus Status { get; set; }

        public virtual Process Process { get; protected set; }

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
