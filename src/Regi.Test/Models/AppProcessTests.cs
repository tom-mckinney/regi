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
        public void OnDispose_is_called_if_set_when_disposing()
        {
            int callCount = 0;

            AppProcess model = new AppProcess(new System.Diagnostics.Process(), AppTask.Test, AppStatus.Running)
            {
                OnKill = (processId) => callCount++
            };

            model.Kill();

            Assert.Equal(1, callCount);
        }

        [Fact]
        public void OnDispose_is_never_invoked_if_it_is_null()
        {
            int callCount = 0;

            AppProcess model = new AppProcess(new System.Diagnostics.Process(), AppTask.Test, AppStatus.Running)
            {
                OnKill = null
            };

            model.Kill();

            Assert.Equal(0, callCount);
        }
    }
}
