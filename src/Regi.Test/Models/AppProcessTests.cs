using Moq;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Regi.Test.Models
{
    public class AppProcessTests
    {
        [Fact]
        public void OnKill_is_called_if_set_when_process_is_killed()
        {
            int callCount = 0;

            AppProcess process = new AppProcess(new System.Diagnostics.Process(), AppTask.Test, AppStatus.Running)
            {
                OnKill = (processId) => callCount++
            };

            process.Kill();

            Assert.Equal(1, callCount);
        }

        [Fact]
        public void OnKill_is_never_invoked_if_it_is_null()
        {
            int callCount = 0;

            AppProcess process = new AppProcess(new System.Diagnostics.Process(), AppTask.Test, AppStatus.Running)
            {
                OnKill = null
            };

            Assert.Equal(0, callCount);
        }
    }
}
