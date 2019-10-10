using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using Regi.Models.Exceptions;
using Regi.Services;
using Regi.Test.Helpers;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    [Collection(TestCollections.NoParallel)]
    public class ConfigurationServiceTests
    {
        private readonly IConsole _console;

        private const int dotnetAppCount = 2;
        private const int nodeAppCount = 1;
        private const int totalAppCount = dotnetAppCount + nodeAppCount;

        private const int dotnetTestCount = 2;
        private const int nodeTestCount = 0;
        private const int totalTestCount = dotnetTestCount + nodeTestCount;

        public ConfigurationServiceTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
        }

        private IConfigurationService CreateService()
        {
            return new ConfigurationService();
        }

        [Fact]
        public void GetStatupConfig_throws_exception_when_directory_not_found()
        {
            DirectoryUtility.SetWorkingDirectory(PathHelper.SampleDirectoryPath("BUNK_DIRECTORY"));

            var service = CreateService();

            Assert.Throws<DirectoryNotFoundException>(() => service.GetConfiguration(null));
        }

        [Fact]
        public void GetStatupConfig_throws_exception_when_startup_config_not_found()
        {
            DirectoryUtility.SetWorkingDirectory(PathHelper.SampleDirectoryPath("SampleAppError"));

            var service = CreateService();

            Assert.Throws<RegiException>(() => service.GetConfiguration(null));
        }

        [Theory]
        [InlineData("ConfigurationBad")]
        [InlineData("ConfigurationWrongEnum")]
        public void GetStatupConfig_throws_exception_when_startup_config_has_bad_format(string configuration)
        {
            DirectoryUtility.SetWorkingDirectory(PathHelper.SampleDirectoryPath(configuration));

            var service = CreateService();

            var ex = Assert.Throws<RegiException>(() => service.GetConfiguration(null));

            Assert.IsType<JsonSerializationException>(ex.InnerException);

            ex.LogAndReturnStatus(_console);
        }

        [Fact]
        public void GetStartupConfig_returns_configuration_model_when_run_in_directory_with_startup_file()
        {
            string expectedPath = PathHelper.SampleDirectoryPath("ConfigurationGood");

            DirectoryUtility.SetWorkingDirectory(expectedPath);

            var service = CreateService();

            StartupConfig startupConfig = service.GetConfiguration(null);

            Assert.StartsWith(expectedPath, startupConfig.Path, StringComparison.InvariantCulture);
            Assert.Equal(totalAppCount, startupConfig.Apps.Count);
            Assert.Equal(totalTestCount, startupConfig.Tests.Count);
            Assert.Empty(startupConfig.Services);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/regi.json")]
        public void GetStartupConfig_will_use_ConfigurationPath_if_specified(string file)
        {
            string expectedPath = PathHelper.SampleDirectoryPath("ConfigurationGood");

            DirectoryUtility.SetWorkingDirectory(PathHelper.SampleDirectoryPath("BUNK_DIRECTORY"));

            var options = new RegiOptions
            {
                ConfigurationPath = expectedPath + file
            };

            var service = CreateService();

            StartupConfig startupConfig = service.GetConfiguration(options);

            Assert.StartsWith(expectedPath, startupConfig.Path, StringComparison.InvariantCulture);
            Assert.Equal(expectedPath, DirectoryUtility.WorkingDirectory);


            Assert.Equal(totalAppCount, startupConfig.Apps.Count);
            Assert.Equal(totalTestCount, startupConfig.Tests.Count);
            Assert.Empty(startupConfig.Services);
        }

        [Fact]
        public void GetStartupConfig_throws_if_ConfigurationPath_is_specified_but_does_not_exist()
        {
            string expectedPath = PathHelper.SampleDirectoryPath("BUNK_DIRECTORY");

            DirectoryUtility.SetWorkingDirectory(PathHelper.SampleDirectoryPath("ConfigurationGood"));

            var options = new RegiOptions
            {
                ConfigurationPath = expectedPath
            };

            var service = CreateService();

            Assert.Throws<DirectoryNotFoundException>(() => service.GetConfiguration(options));
        }
    }
}
