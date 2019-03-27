using Moq;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System.Collections.Generic;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class ParallelServiceTests
    {
        private readonly IParallelService _service;
        private readonly Mock<INetworkingService> _networkingServiceMock;
        private readonly TestConsole _console;

        public ParallelServiceTests(ITestOutputHelper output)
        {
            _networkingServiceMock = new Mock<INetworkingService>();

            _console = new TestConsole(output);
            _service = new ParallelService(new TestConsole(output), _networkingServiceMock.Object);
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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void WaitOnPorts_waits_until_all_required_ports_are_being_listened_on(int expectedCallCount)
        {
            int callCount = 0;
            _networkingServiceMock.Setup(m => m.GetListeningPorts())
                .Callback(() => callCount++)
                .Returns(() =>
                {
                    if (callCount < expectedCallCount)
                        return new IPEndPoint[] { };
                    else
                        return new IPEndPoint[] { new IPEndPoint(00000, 9080) };
                })
                .Verifiable();

            _service.WaitOnPorts(new List<Project> { new Project { Name = "TestProject", Port = 9080 } });

            _networkingServiceMock.Verify(m => m.GetListeningPorts(), Times.Exactly(expectedCallCount));
            Assert.Equal(callCount, expectedCallCount);
        }
    }
}
