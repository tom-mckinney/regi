using Moq;
using Regi.Frameworks;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class CleanupServiceTests
    {
        private readonly Mock<IDotnet> _dotnetServiceMock = new Mock<IDotnet>();
        private readonly TestFileSystem _fileSystem = new TestFileSystem();
        private readonly TestConsole _console;

        public CleanupServiceTests(ITestOutputHelper outputHelper)
        {
            _console = new TestConsole(outputHelper);
        }

        ICleanupService CreateService()
        {
            return new CleanupService(_dotnetServiceMock.Object, _fileSystem, _console);
        }

        [Fact]
        public async Task ShutdownBuildServers_shuts_down_dotnet_build_servers()
        {
            var options = TestOptions.Create();

            var dotnetShutdownBuildServer = new AppProcess(new Process(), AppTask.Cleanup, AppStatus.Success);
            _dotnetServiceMock.Setup(m => m.ShutdownBuildServer(options, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dotnetShutdownBuildServer)
                .Verifiable();

            var service = CreateService();

            var processes = await service.ShutdownBuildServers(options, CancellationToken.None);

            Assert.Equal(1, processes.Count);
            Assert.Same(dotnetShutdownBuildServer, processes[0]);
        }
    }
}
