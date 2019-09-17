using Regi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regi.Test.Helpers
{
    public static class TestOptions
    {
        public static RegiOptions Create(EnvironmentVariableDictionary varList = null, params string[] otherArguments)
        {
            return new RegiOptions
            {
                VariableList = varList,
                RemainingArguments = otherArguments.ToList(),
                Verbose = true,
                KillProcessesOnExit = false
            };
        }
    }
}
