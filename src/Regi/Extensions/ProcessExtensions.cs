using Microsoft.Win32.SafeHandles;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                    startInfo?.EnvironmentVariables.TryAdd(env.Key, env.Value?.ToString());
                }
            }
        }

        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            if (process.HasExited) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);

            cancellationToken.Register(() => tcs.TrySetCanceled());

            return tcs.Task;
        }
    }
}
