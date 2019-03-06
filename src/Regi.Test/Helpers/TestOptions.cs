using Regi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Test.Helpers
{
    public static class TestOptions
    {
        public static CommandOptions Create(VariableList varList = null, string searchPattern = null)
        {
            return new CommandOptions
            {
                VariableList = varList,
                Arguments = searchPattern,
                Verbose = true,
                KillProcessesOnExit = false
            };
        }
    }
}
