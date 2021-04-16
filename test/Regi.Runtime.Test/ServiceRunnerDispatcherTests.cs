using Moq;
using Regi.Abstractions;
using Regi.Abstractions.Options;
using Regi.Test.Stubs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Upstream.Testing;
using Xunit;

namespace Regi.Runtime.Test
{
    public class ServiceRunnerDispatcherTests : TestBase<ServiceRunnerDispatcher>
    {
        private readonly List<IServiceRunner> _serviceRunners = new();

        protected override ServiceRunnerDispatcher CreateTestClass()
        {
            return new ServiceRunnerDispatcher(_serviceRunners);
        }

        [Fact]
        public async Task Dispatches_services_with_matching_type()
        {
            var testService = new TestServiceMultiplexer();
            var options = new OptionsBase();

            var expectedManagedProcess = new StubbedManagedProcess();

            var serviceRunnerMock = new Mock<IServiceRunner>(MockBehavior.Strict);
            serviceRunnerMock.Setup(m => m.Type)
                .Returns(ServiceType.Docker);
            serviceRunnerMock.Setup(m => m.RunAsync(testService, options, CancellationToken.None))
                .ReturnsAsync(expectedManagedProcess);

            _serviceRunners.Add(serviceRunnerMock.Object);

            var actualManagedProcess = await TestClass.DispatchAsync(testService, options, CancellationToken.None);

            Assert.Same(expectedManagedProcess, actualManagedProcess);
        }

        [Fact]
        public async Task Dispatch_throws_if_no_ServiceRunner_for_ServiceType()
        {
            var testService = new TestServiceMultiplexer
            {
                Type = 0 // not a real ServiceType enum value
            };
            var options = new OptionsBase();

            await Assert.ThrowsAsync<NotImplementedException>(() => TestClass.DispatchAsync(testService, options, CancellationToken.None).AsTask());
        }

        private class TestService : IService
        {
            public string Name { get; set; }
            public ServiceType Type { get; set; } = ServiceType.Docker;
        }

        private class TestServiceMultiplexer : IServiceMultiplexer
        {
            public ServiceType Type { get; set; } = ServiceType.Docker;
            public string Name { get; set; }
            public string Image { get; set; }
            public List<string> Ports { get; set; }
            public List<string> Volumes { get; set; }
        }
    }
}
