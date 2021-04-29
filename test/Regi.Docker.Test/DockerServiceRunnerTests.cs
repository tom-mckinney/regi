using Moq;
using Regi.Abstractions;
using Regi.Abstractions.Options;
using Regi.Test.Stubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Upstream.Testing;
using Xunit;

namespace Regi.Docker.Test
{
    public class DockerServiceRunnerTests : TestBase<DockerServiceRunner>
    {
        private readonly Mock<IProcessManager> _processManagerMock = new(MockBehavior.Strict);
        private readonly Mock<IManagedProcess> _managedProcessMock = new(MockBehavior.Strict);

        protected override DockerServiceRunner CreateTestClass()
        {
            return new DockerServiceRunner(_processManagerMock.Object);
        }

        [Fact]
        public async Task Run_creates_and_starts_Docker_process()
        {
            var expectedFileName = "docker";
            var expectedArgs = "run --name backend_db -p 1420:1433 -v dbdata:/var/opt/mssql mcr.microsoft.com/mssql/server";

            _processManagerMock.Setup(m => m.CreateAsync(expectedFileName, expectedArgs, null))
                .ReturnsAsync(_managedProcessMock.Object);

            _managedProcessMock.Setup(m => m.StartAsync(CancellationToken.None))
                .Returns(Task.CompletedTask);

            var service = new DockerService
            {
                Name = "backend_db",
                Image = "mcr.microsoft.com/mssql/server",
                Ports = new() { "1420:1433" },
                Volumes = new() { "dbdata:/var/opt/mssql" },
            };

            var actualManagedProcess = await TestClass.RunAsync(service, new OptionsBase(), CancellationToken.None);

            Assert.Same(_managedProcessMock.Object, actualManagedProcess);
        }
    }
}
