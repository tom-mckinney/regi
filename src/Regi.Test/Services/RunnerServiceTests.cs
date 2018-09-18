using Moq;
using Newtonsoft.Json;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class RunnerServiceTests
    {
        private readonly Mock<IDotnetService> _dotnetServiceMock;
        private readonly Mock<INodeService> _nodeServiceMock;
        private readonly IRunnerService _runnerService;

        private readonly string _startupConfigGood;
        private readonly string _startupConfigBad;
        private readonly char Slash = Path.DirectorySeparatorChar;

        private const int dotnetAppCount = 2;
        private const int nodeAppCount = 1;
        private const int totalAppCount = dotnetAppCount + nodeAppCount;

        public RunnerServiceTests(ITestOutputHelper output)
        {
            _dotnetServiceMock = new Mock<IDotnetService>();
            _nodeServiceMock = new Mock<INodeService>();
            _runnerService = new RunnerService(_dotnetServiceMock.Object, _nodeServiceMock.Object, new TestConsole(output));

            _startupConfigGood = SampleDirectoryPath("ConfigurationGood");
            _startupConfigBad = SampleDirectoryPath("ConfigurationBad");
        }

        [Fact]
        public void GetStatupConfig_throws_exception_when_directory_not_found()
        {
            string bunkDirectory = SampleDirectoryPath("BUNK_DIRECTORY");
            Assert.Throws<DirectoryNotFoundException>(() => _runnerService.GetStartupConfig(bunkDirectory));
        }

        [Fact]
        public void GetStatupConfig_throws_exception_when_startup_config_not_found()
        {
            string directoryWithoutStartup = SampleDirectoryPath("SampleAppError");
            Assert.Throws<FileNotFoundException>(() => _runnerService.GetStartupConfig(directoryWithoutStartup));
        }

        [Fact]
        public void GetStatupConfig_throws_exception_when_startup_config_has_bad_format()
        {
            string badConfigurationDirectory = SampleDirectoryPath("ConfigurationBad");
            Assert.Throws<JsonSerializationException>(() => _runnerService.GetStartupConfig(badConfigurationDirectory));
        }

        [Fact]
        public void GetStartupConfig_returns_configuration_model_when_run_in_directory_with_startup_file()
        {
            StartupConfig startupConfig = _runnerService.GetStartupConfig(_startupConfigGood);

            Assert.Equal(totalAppCount, startupConfig.Apps.Count);
            Assert.Equal(2, startupConfig.Tests.Count);
            Assert.Empty(startupConfig.Services);
        }

        [Fact]
        public void RunAsync_returns_a_process_for_every_app_in_startup_config()
        {
            _dotnetServiceMock.Setup(m => m.RunProject(It.IsAny<FileInfo>(), It.IsAny<bool>(), It.IsAny<int?>()))
                .Returns<FileInfo, bool, int?>((f, b, i) => new AppProcess(new Process(), AppTask.Run, AppStatus.Success, i))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<FileInfo>(), It.IsAny<bool>(), It.IsAny<int?>()))
                .Returns<FileInfo, bool, int?>((f, b, i) => new AppProcess(new Process(), AppTask.Run, AppStatus.Success, i))
                .Verifiable();

            var processes = _runnerService.RunAsync(_startupConfigGood);

            Assert.Equal(totalAppCount, processes.Count);
            Assert.Single(processes, p => p.Port == 9080);

            _dotnetServiceMock.Verify(m => m.RunProject(It.IsAny<FileInfo>(), It.IsAny<bool>(), It.IsAny<int?>()), Times.Exactly(dotnetAppCount));
            _nodeServiceMock.Verify(m => m.StartProject(It.IsAny<FileInfo>(), It.IsAny<bool>(), It.IsAny<int?>()), Times.Exactly(nodeAppCount));
        }

        [Fact]
        public void TestAsync_returns_a_process_for_every_test_in_startup_config()
        {
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<FileInfo>(), It.IsAny<bool>()))
                .Returns(new AppProcess(new Process(), AppTask.Run, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.TestAsync(_startupConfigGood);

            Assert.Equal(2, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Theory]
        [InlineData(ProjectType.Unit, 1)]
        [InlineData(ProjectType.Integration, 1)]
        public void TestAsync_will_only_run_tests_on_type_specified(ProjectType type, int expectedCount)
        {
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<FileInfo>(), It.IsAny<bool>()))
                .Returns(new AppProcess(new Process(), AppTask.Run, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.TestAsync(_startupConfigGood, type);

            Assert.Equal(expectedCount, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        internal string SampleDirectoryPath(string name)
        {
            string path = $"{Directory.GetCurrentDirectory()}{Slash}_SampleProjects_{Slash}{name}";

            return path;
        }
    }
}
