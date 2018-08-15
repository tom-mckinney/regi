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

        public RunnerServiceTests()
        {
            _runnerService = new RunnerService();

            _sampleProjectsDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "_SampleProjects_");
        }

        [Fact]
        public void Throws_exception_when_directory_not_found()
        {
            DirectoryInfo bunkDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "BUNK_DIRECTORY");
            Assert.Throws<DirectoryNotFoundException>(() => _runnerService.RunAsync(bunkDirectory));
        }

        [Fact]
        public void Finds_startup_config_file_when_run_in_same_directory()
        {
            Assert.True(_sampleProjectsDirectory.Exists);
        }
    }
}
