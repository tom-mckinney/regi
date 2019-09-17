using Microsoft.Win32.SafeHandles;
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
        public static void CopyEnvironmentVariables(this ProcessStartInfo startInfo, EnvironmentVariableDictionary varList)
        {
            if (varList?.Any() == true)
            {
                foreach (var env in varList)
                {
                    startInfo?.EnvironmentVariables.TryAdd(env.Key, env.Value);
                }
            }
        }

        
    }
}
