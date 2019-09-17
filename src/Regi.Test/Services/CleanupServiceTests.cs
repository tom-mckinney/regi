using Moq;
using Regi.Models;
using Regi.Services;
using Regi.Services.Frameworks;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class CleanupServiceTests
    {
        Mock<IDotnetService> dotnetServiceMock = new Mock<IDotnetService>();
        private readonly TestConsole console;

        public CleanupServiceTests(ITestOutputHelper outputHelper)
        {
            this.console = new TestConsole(outputHelper);
        }

        ICleanupService CreateService()
        {
            return new CleanupService(dotnetServiceMock.Object, console);
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
