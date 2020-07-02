using Moq;
using Regi.Services;
using Regi.Test.Helpers;
using Regi.Utilities;
using System;
using System.IO;
using System.Net.Sockets;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class NetworkingServiceTests
    {
        private readonly TestConsole _console;
        private readonly TestFileSystem _fileSystem = new TestFileSystem();
        private readonly Mock<IRuntimeInfo> _runtimeInfoMock = new Mock<IRuntimeInfo>(MockBehavior.Strict);

        public NetworkingServiceTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
        }

        private INetworkingService CreateService()
        {
            return new NetworkingService(_fileSystem, _runtimeInfoMock.Object, _console);
        }

        [Fact]
        public void IsPortListening_gets_list_of_all_active_ports()
        {
            var realRuntimeInfo = new RuntimeInfo();

            _runtimeInfoMock.Setup(m => m.IsWindowsLinux).Returns(realRuntimeInfo.IsWindowsLinux);
            _runtimeInfoMock.Setup(m => m.IsWindows).Returns(realRuntimeInfo.IsWindows);
            _runtimeInfoMock.Setup(m => m.IsMac).Returns(realRuntimeInfo.IsMac);
            _runtimeInfoMock.Setup(m => m.NewLine).Returns(realRuntimeInfo.NewLine);

            int port = new Random().Next(10000, 30000);

            var listener = TcpListener.Create(port);

            listener.Start();

            var service = CreateService();

            Assert.True(service.IsPortListening(port));

            listener.Stop();
        }

        [Theory]
        [InlineData(8080, true, "MacNetstat.txt")]
        [InlineData(8888, false, "MacNetstat.txt")]
        [InlineData(53483, false, "MacNetstat.txt")]
        [InlineData(9000, false, "MacNetstat-CLOSE_WAIT.txt")]
        public void ContainsPort_Mac_returns_true_if_port_is_listening(int port, bool isListening, string fileName)
        {
            FileInfo responseFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "SystemResponses", "Mac", fileName));

            if (!responseFile.Exists)
                throw new FileNotFoundException($"Could not find test file: {responseFile.FullName}");

            _runtimeInfoMock.Setup(m => m.NewLine).Returns("\n").Verifiable();
            _runtimeInfoMock.Setup(m => m.IsMac).Returns(true).Verifiable();

            string netstatResponse = File.ReadAllText(responseFile.FullName);

            NetworkingService service = (NetworkingService)CreateService();

            Assert.Equal(isListening, service.ContainsNetstatPort(netstatResponse, port));

            _runtimeInfoMock.Verify();
        }
    }
}
