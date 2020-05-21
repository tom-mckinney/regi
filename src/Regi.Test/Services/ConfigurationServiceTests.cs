using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    [Collection(TestCollections.NoParallel)]
    public class ConfigurationServiceTests
    {
        private readonly TestFileSystem _fileSystem = new TestFileSystem();
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
            return new ConfigurationService(_fileSystem);
        }

        [Theory]
        [InlineData(3, 3)]
        [InlineData(3, 0)]
        [InlineData(0, 3)]
        public async Task CreateStartupConfig_sorts_projects_into_categories(int appCount, int testCount)
        {
            var projects = new List<Project>();

            for (int i = 0; i < appCount; i++)
            {
                projects.Add(SampleProjects.Backend);
            }
            for (int i = 0; i < testCount; i++)
            {
                projects.Add(SampleProjects.XunitTests);
            }

            var service = CreateService();

            var config = await service.CreateConfigurationAsync(projects, TestOptions.Create());

            Assert.Equal(appCount + testCount, config.Projects.Count);
            Assert.Equal(appCount, config.Projects.Count(p => p.Name == SampleProjects.Backend.Name));
            Assert.Equal(testCount, config.Projects.Count(p => p.Name == SampleProjects.XunitTests.Name));
        }

        [Fact]
        public async Task GetStartupConfig_returns_configuration_model_when_run_in_directory_with_startup_file()
        {
            string expectedPath = PathHelper.GetSampleProjectPath("ConfigurationGood");

            _fileSystem.WorkingDirectory = expectedPath;

            var service = CreateService();

            StartupConfig startupConfig = await service.GetConfigurationAsync(null);

            Assert.StartsWith(expectedPath, startupConfig.Path, StringComparison.InvariantCulture);
            Assert.Equal(totalAppCount + totalTestCount, startupConfig.Projects.Count);
            Assert.Equal(totalAppCount, startupConfig.Projects.WhereApp().Count());
            Assert.Equal(totalTestCount, startupConfig.Projects.WhereTest().Count());
            Assert.Empty(startupConfig.Services);

            AssertAllRuntimePropertiesAreBound(startupConfig, expectedPath);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/regi.json")]
        public async Task GetStartupConfig_will_use_ConfigurationPath_if_specified(string file)
        {
            string expectedPath = PathHelper.GetSampleProjectPath("ConfigurationGood");

            _fileSystem.WorkingDirectory = PathHelper.GetSampleProjectPath("BUNK_DIRECTORY");

            var options = new CommandOptions
            {
                ConfigurationPath = expectedPath + file
            };

            var service = CreateService();

            StartupConfig startupConfig = await service.GetConfigurationAsync(options);

            Assert.StartsWith(expectedPath, startupConfig.Path, StringComparison.InvariantCulture);
            Assert.Equal(expectedPath, _fileSystem.WorkingDirectory);


            Assert.Equal(totalAppCount + totalTestCount, startupConfig.Projects.Count);
            Assert.Equal(totalAppCount, startupConfig.Projects.WhereApp().Count());
            Assert.Equal(totalTestCount, startupConfig.Projects.WhereTest().Count());
            Assert.Empty(startupConfig.Services);

            AssertAllRuntimePropertiesAreBound(startupConfig, expectedPath);
        }

        private void AssertAllRuntimePropertiesAreBound(StartupConfig config, string expectedPath)
        {
            Assert.StartsWith(expectedPath, config.Path, StringComparison.InvariantCulture);

            foreach (var project in config.Projects)
            {
                foreach (var path in project.GetAppDirectoryPaths(_fileSystem))
                {
                    Directory.Exists(path);
                }
            }
        }

        [Fact]
        public async Task GetStartupConfig_throws_if_ConfigurationPath_is_specified_but_does_not_exist()
        {
            string expectedPath = PathHelper.GetSampleProjectPath("BUNK_DIRECTORY");

            _fileSystem.WorkingDirectory = PathHelper.GetSampleProjectPath("ConfigurationGood");

            var options = new CommandOptions
            {
                ConfigurationPath = expectedPath
            };

            var service = CreateService();

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => service.GetConfigurationAsync(options));
        }

        [Fact]
        public async Task GetStartupConfig_throws_when_using_deprecated_config_file()
        {
            _fileSystem.WorkingDirectory = PathHelper.GetSampleProjectPath("ConfigurationDeprecated");

            var service = CreateService();

            var ex = await Assert.ThrowsAsync<RegiException>(() => service.GetConfigurationAsync(null));

            Assert.IsType<RegiException>(ex.InnerException);
            Assert.Equal("The properties \"apps\" and \"tests\" have been removed. Use \"projects\" instead.", ex.InnerException.Message);

            ex.LogAndReturnStatus(_console);
        }

        [Fact]
        public async Task GetStatupConfig_throws_exception_when_directory_not_found()
        {
            _fileSystem.WorkingDirectory = PathHelper.GetSampleProjectPath("BUNK_DIRECTORY");

            var service = CreateService();

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => service.GetConfigurationAsync(null));
        }

        [Fact]
        public async Task GetStatupConfig_throws_exception_when_startup_config_not_found()
        {
            _fileSystem.WorkingDirectory = PathHelper.GetSampleProjectPath("SampleAppError");

            var service = CreateService();

            await Assert.ThrowsAsync<RegiException>(() => service.GetConfigurationAsync(null));
        }

        [Fact]
        public async Task GetStatupConfig_throws_exception_when_config_uses_an_invalid_enum()
        {
            _fileSystem.WorkingDirectory = PathHelper.GetSampleProjectPath("ConfigurationWrongEnum");

            var service = CreateService();

            var ex = await Assert.ThrowsAsync<RegiException>(() => service.GetConfigurationAsync(null));

            Assert.IsType<System.Text.Json.JsonException>(ex.InnerException);

            ex.LogAndReturnStatus(_console);
        }

        [Fact(Skip = "Enable this when .NET 5 serializer supports required properties")]
        public async Task GetConfiguration_throws_when_required_properties_are_missing()
        {
            _fileSystem.WorkingDirectory = PathHelper.GetSampleProjectPath("ConfigurationBad");

            var service = CreateService();

            var ex = await Assert.ThrowsAsync<RegiException>(() => service.GetConfigurationAsync(null));

            Assert.IsType<System.Text.Json.JsonException>(ex.InnerException);

            ex.LogAndReturnStatus(_console);
        }
    }
}
