using Moq;
using Newtonsoft.Json;
using Regiment.Models;
using Regiment.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace Regiment.Test.Services
{
    public class RunnerServiceTests
    {
        private readonly Mock<IDotnetService> _dotnetServiceMock;
        private readonly IRunnerService _runnerService;

        private readonly string _startupConfigGood;
        private readonly string _startupConfigBad;
        private readonly char Slash = Path.DirectorySeparatorChar;

        public RunnerServiceTests()
        {
            _dotnetServiceMock = new Mock<IDotnetService>();
            _runnerService = new RunnerService(_dotnetServiceMock.Object);

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

            Assert.Single(startupConfig.Apps);
            Assert.Single(startupConfig.Tests);
            Assert.Empty(startupConfig.Services);
        }

        [Fact]
        public void RunAsync_returns_a_process_for_every_app_in_startup_config()
        {
            _dotnetServiceMock.Setup(m => m.RunProject(It.IsAny<FileInfo>(), It.IsAny<bool>()))
                .Returns(new DotnetProcess(new Process(), DotnetTask.Run, DotnetStatus.Success))
                .Verifiable();

            var processes = _runnerService.RunAsync(_startupConfigGood);

            Assert.Single(processes);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public void TestAsync_returns_a_process_for_every_test_in_startup_config()
        {
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<FileInfo>(), It.IsAny<bool>()))
                .Returns(new DotnetProcess(new Process(), DotnetTask.Run, DotnetStatus.Success))
                .Verifiable();

            var processes = _runnerService.TestAsync(_startupConfigGood);

            Assert.Single(processes);

            _dotnetServiceMock.VerifyAll();
        }

        internal string SampleDirectoryPath(string name)
        {
            string path = $"{Directory.GetCurrentDirectory()}{Slash}_SampleProjects_{Slash}{name}";

            return path;
        }
    }
}
