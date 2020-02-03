using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Models
{
    public enum CommandStatus
    {
        Failed = 1,
        Success
    }

    public class CommandOutput
    {
        public CommandStatus Status { get; set; }

        public int ProcessCount { get; set; }
    }
}
