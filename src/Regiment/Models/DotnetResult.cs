using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regiment.Models
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

    public class DotnetProcess
    {
        public DotnetProcess(Process process, DotnetTask task, DotnetStatus status)
        {
            Process = process;
            Task = task;
            Status = status;
            Start = process.StartTime;
            End = DateTimeOffset.UtcNow;
        }

        public DotnetTask Task { get; set; }

        public DotnetStatus Status { get; set; }

        public virtual Process Process { get; }

        public DateTimeOffset Start { get; set; }

        public DateTimeOffset End { get; set; }
    }
}
