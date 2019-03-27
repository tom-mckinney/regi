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
                OnDispose = (processId) => callCount++
            };

            model.Dispose();

            Assert.Equal(1, callCount);
        }

        [Fact]
        public void OnDispose_is_never_invoked_if_it_is_null()
        {
            int callCount = 0;

            AppProcess model = new AppProcess(new System.Diagnostics.Process(), AppTask.Test, AppStatus.Running)
            {
                OnDispose = null
            };

            model.Dispose();

            Assert.Equal(0, callCount);
        }
    }
}
