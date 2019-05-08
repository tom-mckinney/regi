using Regi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Test.Helpers
{
    public static class TestOptions
    {
        public static CommandOptions Create(VariableList varList = null, params string[] otherArguments)
        {
            return new CommandOptions
            {
                VariableList = varList,
                RemainingArguments = otherArguments,
                Verbose = true,
                KillProcessesOnExit = false
            };
        }
    }
}
