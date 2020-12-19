using Moq;
using Regi.Abstractions;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    [Collection(TestCollections.NoParallel)]
    public class QueueServiceTests
    {
        private readonly IQueueService _service;
        private readonly Mock<INetworkingService> _networkingServiceMock = new Mock<INetworkingService>(MockBehavior.Strict);
        private readonly TestConsole _console;

        public QueueServiceTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
            _service = new QueueService(new TestConsole(output), _networkingServiceMock.Object);
        }

        [Fact]
        public async Task Queue_adds_to_async_by_default_and_serial_if_specified()
        {
            int taskCount = 5;
            int asyncExecutions = 0;
            int serialExecutions = 0;

            for (int i = 0; i < taskCount; i++)
            {
                _service.Queue(false, () =>
                {
                    Interlocked.Increment(ref asyncExecutions);
                }, CancellationToken.None);

                _service.Queue(true, () =>
                {
                    serialExecutions++;
                }, CancellationToken.None);
            }

            await _service.RunAllAsync(CancellationToken.None);

            Assert.Equal(taskCount, asyncExecutions);
            Assert.Equal(taskCount, ((QueueService)_service).AsyncActions.Count);

            Assert.Equal(taskCount, serialExecutions);
            Assert.Equal(taskCount, ((QueueService)_service).SerialActions.Count);
        }

        [Fact]
        public async Task QueueAsync_can_add_and_run_all_tasks()
        {
            int taskCount = 5;
            int executions = 0;

            for (int i = 0; i < taskCount; i++)
            {
                _service.QueueAsync(() =>
                {
                    Interlocked.Increment(ref executions);
                }, CancellationToken.None);
            }

            await _service.RunAllAsync(CancellationToken.None);

            Assert.Equal(taskCount, executions);
            Assert.Equal(taskCount, ((QueueService)_service).AsyncActions.Count);
        }

        [Fact]
        public async Task QueueSerial_can_add_and_run_all_tasks()
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

            await _service.RunAllAsync(CancellationToken.None);

            Assert.Equal(taskCount, executions);
            Assert.Equal(taskCount, ((QueueService)_service).SerialActions.Count);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task Queue_adds_and_runs_serial_actions_after_async_actions(int taskCount)
        {
            int asyncRunCount = 0;
            int serialRunCount = 0;

            _service.QueueAsync(() => Interlocked.Increment(ref asyncRunCount), CancellationToken.None);
            _service.QueueAsync(() => Interlocked.Increment(ref asyncRunCount), CancellationToken.None);

            for (int i = 0; i < taskCount; i++)
            {
                _service.QueueSerial(() => serialRunCount++);
            }

            await _service.RunAllAsync(CancellationToken.None);

            Assert.Equal(2, asyncRunCount);
            Assert.Equal(taskCount, serialRunCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task WaitOnPorts_waits_until_all_required_ports_are_being_listened_on(int expectedCallCount)
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

            await _service.ConfirmProjectsStartedAsync(new List<IProject> { new Project { Name = "TestProject", Port = port } }, CancellationToken.None);

            _networkingServiceMock.Verify(m => m.IsPortListening(port), Times.Exactly(expectedCallCount));
            Assert.Equal(callCount, expectedCallCount);
        }

        [Fact]
        public async Task RunAsync_invokes_all_async_actions_asynchronously()
        {
            int callCount = 0;

            _service.QueueAsync(() => Interlocked.Increment(ref callCount), CancellationToken.None);
            _service.QueueAsync(() => Interlocked.Increment(ref callCount), CancellationToken.None);
            _service.QueueAsync(() => Interlocked.Increment(ref callCount), CancellationToken.None);

            await ((QueueService)_service).RunAsyncActions(CancellationToken.None);

            Assert.Equal(3, callCount);
        }

        [Fact]
        public async Task RunSerial_invokes_all_serial_actions_in_order_added()
        {
            int callCount = 0;

            _service.QueueSerial(() => callCount = 1);
            _service.QueueSerial(() => callCount = 2);
            _service.QueueSerial(() => callCount = 3);

            await ((QueueService)_service).RunSerialActions(CancellationToken.None);

            Assert.Equal(3, callCount);
        }

        [Fact]
        public async Task RunAll_runs_all_async_actions_and_then_all_serial_actions()
        {
            int asyncCount = 0;
            int serialCount = 0;

            _service.QueueAsync(() => Interlocked.Increment(ref asyncCount), CancellationToken.None);
            _service.QueueAsync(() => Interlocked.Increment(ref asyncCount), CancellationToken.None);
            _service.QueueAsync(() => Interlocked.Increment(ref asyncCount), CancellationToken.None);

            _service.QueueSerial(() => serialCount++);
            _service.QueueSerial(() => serialCount++);
            _service.QueueSerial(() => serialCount++);

            await _service.RunAllAsync(CancellationToken.None);

            Assert.Equal(3, asyncCount);
            Assert.Equal(3, serialCount);
        }
    }
}
