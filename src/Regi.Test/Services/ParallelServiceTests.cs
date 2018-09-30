using Regi.Services;
using Regi.Test.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class ParallelServiceTests
    {
        private readonly IParallelService _service;
        private readonly TestConsole _console;

        public ParallelServiceTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
            _service = new ParallelService();
        }

        [Fact]
        public void Queue_can_add_and_run_all_tasks()
        {
            int taskCount = 5;
            int executions = 0;

            for (int i = 0; i < taskCount; i++)
            {
                _service.Queue(() =>
                {
                    executions++;
                });
            }

            _service.RunInParallel();

            Assert.Equal(taskCount, executions);
        }
    }
}
