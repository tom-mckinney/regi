using Regi.Abstractions;
using Regi.Models;
using Regi.Services;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Regi.Test.Models
{
    public class AppProcessTests
    {
        [Fact]
        public void OnKill_is_called_if_set_when_process_is_killed()
        {
            int callCount = 0;

            AppProcess process = new AppProcess(new Process(), AppTask.Test, AppStatus.Running)
            {
                OnKill = (processId) => callCount++
            };

            process.Kill();

            Assert.Equal(1, callCount);
        }

        [Fact]
        public async Task WaitForExitAsync_throws_if_process_is_null()
        {
            AppProcess process = new(null, AppTask.Test, AppStatus.Running);

            await Assert.ThrowsAsync<NullReferenceException>(() => process.WaitForExitAsync(CancellationToken.None));
        }

        [Fact]
        public async Task WaitForExitAsync_can_be_cancelled_by_cancellation_token()
        {
            var runtimeInfo = new RuntimeInfo();
            Process longRunningProcess = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = runtimeInfo.IsWindows ? "cmd" : "bash",
                    Arguments = runtimeInfo.IsWindows ? "timeout 15" : "sleep 15",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            AppProcess process = new AppProcess(longRunningProcess, AppTask.Test, AppStatus.Running);


            process.Start();

            var cts = new CancellationTokenSource();

            var task = process.WaitForExitAsync(cts.Token);

            cts.Cancel();

            try
            {
                await task;
            }
            catch (Exception e)
            {
                Assert.IsType<TaskCanceledException>(e);
            }

            Assert.True(process.IsCanceled);
        }
    }
}
