using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Test.Helpers
{
    internal class TestQueueService : QueueService
    {
        public TestQueueService(IConsole console) : base(console, null) // No networking in Test stub
        {
        }

        public IList<int> ActivePorts { get; } = new List<int>();

        public int WaitOnPortListCallCount = 0;
        public int WaitOnPortCallCount = 0;

        public override void ConfirmProjectsStarted(IDictionary<int, Project> projects)
        {
            WaitOnPortListCallCount++;

            foreach (var p in projects)
            {
                ActivePorts.Add(p.Key);
            }
        }

        public override void WaitOnPort(Project project)
        {
            if (project.Port.HasValue)
            {
                WaitOnPortCallCount++;

                ActivePorts.Add(project.Port.Value);
            }
        }
    }
}
