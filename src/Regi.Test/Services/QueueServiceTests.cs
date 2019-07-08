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
    [Collection("Parallel")]
    public class QueueServiceTests
    {
        private readonly IQueueService _service;
        private readonly Mock<INetworkingService> _networkingServiceMock = new Mock<INetworkingService>(MockBehavior.Strict);
        private readonly TestConsole _console;
        private readonly object _lock = new object();

        public QueueServiceTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
            _service = new QueueService(new TestConsole(output), _networkingServiceMock.Object);
        }

        [Fact]
        public void Queue_adds_to_parallel_by_default_and_serial_if_specified()
        {
            lock (_lock)
            {
                int taskCount = 5;
                int parallelExecutions = 0;
                int serialExecutions = 0;

                for (int i = 0; i < taskCount; i++)
                {
                    _service.Queue(false, () =>
                    {
                        parallelExecutions++;
                    });

                    _service.Queue(true, () =>
                    {
                        serialExecutions++;
                    });
                }

                _service.RunAll();

                Assert.Equal(taskCount, parallelExecutions);
                Assert.Equal(taskCount, ((QueueService)_service).ParallelActions.Count);

                Assert.Equal(taskCount, serialExecutions);
                Assert.Equal(taskCount, ((QueueService)_service).SerialActions.Count);
            }
        }

        [Fact]
        public void QueueParallel_can_add_and_run_all_tasks()
        {
            lock (_lock)
            {
                int taskCount = 5;
                int executions = 0;

                for (int i = 0; i < taskCount; i++)
                {
                    _service.QueueParallel(() =>
                    {
                        executions++;
                    });
                }

                _service.RunAll();

                Assert.Equal(taskCount, executions);
                Assert.Equal(taskCount, ((QueueService)_service).ParallelActions.Count);
            }
        }

        [Fact]
        public void QueueSerial_can_add_and_run_all_tasks()
        {
            int taskCount = 5;
            int executions = 0;

            for (int i = 0; i < taskCount; i++)
            {
                _service.QueueSerial(() =>
                {
                    executions++;
                });
            }

            _service.RunAll();

            Assert.Equal(taskCount, executions);
            Assert.Equal(taskCount, ((QueueService)_service).SerialActions.Count);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void Queue_adds_and_runs_serial_actions_after_parallel_actions(int taskCount)
        {
            int parallelRunCount = 0;
            int serialRunCount = 0;

            _service.QueueParallel(() => parallelRunCount++);
            _service.QueueParallel(() => parallelRunCount++);

            for (int i = 0; i < taskCount; i++)
            {
                _service.QueueSerial(() => serialRunCount++);
            }

            _service.RunAll();

            Assert.Equal(2, parallelRunCount);
            Assert.Equal(taskCount, serialRunCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void WaitOnPorts_waits_until_all_required_ports_are_being_listened_on(int expectedCallCount)
        {
            int port = 9080;

            int callCount = 0;
            _networkingServiceMock.Setup(m => m.IsPortListening(port))
                .Callback(() => callCount++)
                .Returns(() =>
                {
                    if (callCount < expectedCallCount)
                        return false;
                    else
                        return true;
                })
                .Verifiable();

            _service.ConfirmProjectsStarted(new List<Project> { new Project { Name = "TestProject", Port = port } });

            _networkingServiceMock.Verify(m => m.IsPortListening(port), Times.Exactly(expectedCallCount));
            Assert.Equal(callCount, expectedCallCount);
        }

        [Fact]
        public void RunParallel_invokes_all_parallel_actions_in_parallel()
        {
            int callCount = 0;

            _service.QueueParallel(() => callCount++);
            _service.QueueParallel(() => callCount++);
            _service.QueueParallel(() => callCount++);

            _service.RunParallel();

            Assert.Equal(3, callCount);
        }

        [Fact]
        public void RunSerial_invokes_all_serial_actions_in_order_added()
        {
            int callCount = 0;

            _service.QueueSerial(() => callCount = 1);
            _service.QueueSerial(() => callCount = 2);
            _service.QueueSerial(() => callCount = 3);

            _service.RunSerial();

            Assert.Equal(3, callCount);
        }

        [Fact]
        public void RunAll_runs_all_parallel_actions_and_then_all_serial_actions()
        {
            int parallelCount = 0;
            int serialCount = 0;

            _service.QueueParallel(() => parallelCount++);
            _service.QueueParallel(() => parallelCount++);
            _service.QueueParallel(() => parallelCount++);

            _service.QueueSerial(() => serialCount++);
            _service.QueueSerial(() => serialCount++);
            _service.QueueSerial(() => serialCount++);

            _service.RunAll();

            Assert.Equal(3, parallelCount);
            Assert.Equal(3, serialCount);
        }
    }
}
