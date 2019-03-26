using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Extensions
{
    public static class ProcessExtensions
    {
        public static void CopyEnvironmentVariables(this ProcessStartInfo startInfo, VariableList varList)
        {
            if (varList != null && varList.Any())
            {
                foreach (var env in varList)
                {
                    startInfo.EnvironmentVariables.Add(env.Key, env.Value);
                }
            }
        }
    }
}
