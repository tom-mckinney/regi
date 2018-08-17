using Newtonsoft.Json;
using Regiment.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Regiment.Test.Services
{
    public class RunnerServiceTests
    {
        private readonly IRunnerService _runnerService;
        private readonly DirectoryInfo _sampleProjectsDirectory;

        private readonly char Slash = Path.DirectorySeparatorChar;

        public RunnerServiceTests()
        {
            _runnerService = new RunnerService();

            _sampleProjectsDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + Slash + "_SampleProjects_");
        }

        [Fact]
        public void Throws_exception_when_directory_not_found()
        {
            DirectoryInfo bunkDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + Slash + "BUNK_DIRECTORY");
            Assert.Throws<DirectoryNotFoundException>(() => _runnerService.RunAsync(bunkDirectory));
        }

        [Fact]
        public void Throws_exception_when_startup_config_not_found()
        {
            DirectoryInfo directoryWithoutStartup = new DirectoryInfo(Directory.GetCurrentDirectory() + Slash + "_SampleProjects_" + Slash + "SampleAppError");
            Assert.Throws<FileNotFoundException>(() => _runnerService.RunAsync(directoryWithoutStartup));
        }

        [Fact]
        public void Throws_exception_when_startup_config_has_bad_format()
        {
            DirectoryInfo badConfigurationDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + Slash + "_SampleProjects_" + Slash + "ConfigurationBad");
            Assert.Throws<JsonSerializationException>(() => _runnerService.RunAsync(badConfigurationDirectory));
        }

        [Fact]
        public void Finds_startup_config_file_when_run_in_same_directory()
        {
            var processes = _runnerService.RunAsync(_sampleProjectsDirectory);

            Assert.True(processes.Count == 5);
        }
    }
}
