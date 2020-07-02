using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Test.Helpers
{
    internal class TestQueueService : QueueService
    {
        public TestQueueService(IConsole console) : base(console, null) // No networking in Test stub
        {
        }

        public ConcurrentBag<int> ActivePorts { get; } = new ConcurrentBag<int>();

        public int WaitOnPortListCallCount = 0;
        public int WaitOnPortCallCount = 0;

        public override Task ConfirmProjectsStartedAsync(IDictionary<int, Project> projects, CancellationToken cancellationToken)
        {
            WaitOnPortListCallCount++;

            foreach (var p in projects)
            {
                ActivePorts.Add(p.Key);
            }

            return Task.CompletedTask;
        }

        public override Task WaitOnPortAsync(Project project, CancellationToken cancellationToken)
        {
            if (project.Port.HasValue)
            {
                WaitOnPortCallCount++;

                ActivePorts.Add(project.Port.Value);
            }

            return Task.CompletedTask;
        }
    }
}
