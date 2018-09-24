using Regi.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Test.Helpers
{
    internal class TestParallelService : IParallelService
    {
        public IList<Action> Actions { get; } = new List<Action>();

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
    }
}
