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
            DirectoryUtility.SetTargetDirectory(PathHelper.SampleDirectoryPath("BUNK_DIRECTORY"));

            var service = CreateService();

            Assert.Throws<DirectoryNotFoundException>(() => service.GetConfiguration());
        }

        [Fact]
        public void GetStatupConfig_throws_exception_when_startup_config_not_found()
        {
            DirectoryUtility.SetTargetDirectory(PathHelper.SampleDirectoryPath("SampleAppError"));

            var service = CreateService();

            Assert.Throws<RegiException>(() => service.GetConfiguration());
        }

        [Theory]
        [InlineData("ConfigurationBad")]
        [InlineData("ConfigurationWrongEnum")]
        public void GetStatupConfig_throws_exception_when_startup_config_has_bad_format(string configuration)
        {
            DirectoryUtility.SetTargetDirectory(PathHelper.SampleDirectoryPath(configuration));

            var service = CreateService();

            var ex = Assert.Throws<RegiException>(() => service.GetConfiguration());

            Assert.IsType<JsonSerializationException>(ex.InnerException);

            ex.LogAndReturnStatus(_console);
        }

        [Fact]
        public void GetStartupConfig_returns_configuration_model_when_run_in_directory_with_startup_file()
        {
            DirectoryUtility.SetTargetDirectory(PathHelper.SampleDirectoryPath("ConfigurationGood"));

            var service = CreateService();

            StartupConfig startupConfig = service.GetConfiguration();

            Assert.Equal(totalAppCount, startupConfig.Apps.Count);
            Assert.Equal(totalTestCount, startupConfig.Tests.Count);
            Assert.Empty(startupConfig.Services);
        }
    }
}
