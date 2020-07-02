using Regi.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Regi.Test.Models
{
    public class ProjectTests
    {
        [Fact]
        public void OutputStatus_Success_if_all_processes_have_success_status()
        {
            var project = new Project
            {
                Processes = new ConcurrentBag<AppProcess>
                {
                    new AppProcess(null, AppTask.Test, AppStatus.Success),
                    new AppProcess(null, AppTask.Test, AppStatus.Success)
                }
            };

            Assert.Equal(AppStatus.Success, project.OutputStatus);
        }

        [Fact]
        public void OutputStatus_Failure_if_one_or_more_processes_has_failure_status()
        {
            var project = new Project
            {
                Processes = new ConcurrentBag<AppProcess>
                {
                    new AppProcess(null, AppTask.Test, AppStatus.Failure), // This one failed
                    new AppProcess(null, AppTask.Test, AppStatus.Success),
                    new AppProcess(null, AppTask.Test, AppStatus.Running), // This is ignored since failure is higher priority
                    new AppProcess(null, AppTask.Test, AppStatus.Unknown), // This is ignored since failure is higher priority
                }
            };

            Assert.Equal(AppStatus.Failure, project.OutputStatus);
        }

        [Fact]
        public void OutputStatus_Running_if_one_or_more_processes_has_running_status()
        {
            var project = new Project
            {
                Processes = new ConcurrentBag<AppProcess>
                {
                    new AppProcess(null, AppTask.Test, AppStatus.Running), // This one is still running
                    new AppProcess(null, AppTask.Test, AppStatus.Success)
                }
            };

            Assert.Equal(AppStatus.Running, project.OutputStatus);
        }

        [Fact]
        public void OutputStatus_Unknown_if_one_or_more_processes_has_unknown_status()
        {
            var project = new Project
            {
                Processes = new ConcurrentBag<AppProcess>
                {
                    new AppProcess(null, AppTask.Test, AppStatus.Unknown), // This one is unknown
                    new AppProcess(null, AppTask.Test, AppStatus.Success)
                }
            };

            Assert.Equal(AppStatus.Unknown, project.OutputStatus);
        }
    }
}
