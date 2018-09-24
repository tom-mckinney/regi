using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Services
{
    public interface IParallelService
    {
        void Queue(Action action);

        void RunInParallel();
    }

    public class ParallelService : IParallelService
    {
        private readonly IList<Action> actions = new List<Action>();

        public void Queue(Action action)
        {
            actions.Add(action);
        }

        public void RunInParallel()
        {
            Parallel.Invoke(actions.ToArray());
        }
    }
}
