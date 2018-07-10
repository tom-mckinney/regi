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

    public enum DotnetResult
    {
        Failure = 1,
        Success,
    }

    public class DotnetProcess
    {
        public DotnetProcess(Process process, DotnetTask task)
        {
            Process = process;
            Task = task;
            Start = DateTimeOffset.Now;
        }

        public DotnetTask Task { get; set; }

        public DotnetResult Result { get; set; }

        public virtual Process Process { get; }

        public DateTimeOffset Start { get; set; }

        public DateTimeOffset End { get; set; }
    }
}
