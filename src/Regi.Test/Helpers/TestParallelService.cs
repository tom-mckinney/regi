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
    internal class TestParallelService : ParallelService
    {
        public TestParallelService(IConsole console) : base(console, null) // No networking in Test stub
        {
        }

        public IList<int> ActivePorts { get; } = new List<int>();

        public override void WaitOnPorts(IDictionary<int, Project> projects)
        {
            foreach (var p in projects)
            {
                ActivePorts.Add(p.Key);
            }
        }
    }
}
