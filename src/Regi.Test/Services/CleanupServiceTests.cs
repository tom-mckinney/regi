using Moq;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace Regi.Test.Services
{
    public class CleanupServiceTests
    {
        Mock<IDotnetService> dotnetServiceMock = new Mock<IDotnetService>();

        ICleanupService CreateService()
        {
            return new CleanupService(dotnetServiceMock.Object);
        }

        [Fact]
        public void ShutdownBuildServers_shuts_down_dotnet_build_servers()
        {
            var options = TestOptions.Create();

            var dotnetShutdownBuildServer = new AppProcess(new Process(), AppTask.Cleanup, AppStatus.Success);
            dotnetServiceMock.Setup(m => m.ShutdownBuildServer(options))
                .Returns(dotnetShutdownBuildServer)
                .Verifiable();

            var service = CreateService();

            var processes = service.ShutdownBuildServers(options);

            Assert.Equal(1, processes.Count);
            Assert.Same(dotnetShutdownBuildServer, processes[0]);
        }
    }
}
