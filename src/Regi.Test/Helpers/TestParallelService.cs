using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Regi.Test.Helpers
{
    internal class TestParallelService : IParallelService
    {
        public IList<Action> Actions { get; } = new List<Action>();
        public IList<int> ActivePorts { get; } = new List<int>();

        public void Queue(Action action)
        {
            Actions.Add(action);
        }

        public void RunInParallel()
        {
            foreach (var action in Actions)
            {
                action();
            }
        }

        public void WaitOnPorts(IList<Project> projects)
        {
            IDictionary<int, Project> projectsWithPorts = projects
                .Where(p => p.Port.HasValue)
                .ToDictionary(p => p.Port.Value);

            WaitOnPorts(projectsWithPorts);
        }

        public void WaitOnPorts(IDictionary<int, Project> projects)
        {
            foreach (var p in projects)
            {
                ActivePorts.Add(p.Key);
            }
        }
    }
}
