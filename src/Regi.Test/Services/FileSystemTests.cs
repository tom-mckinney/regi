using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    [Collection(TestCollections.NoParallel)]
    public class FileSystemTests
    {
        private IFileSystem _service;

        public FileSystemTests(ITestOutputHelper output)
        {
            _service = new FileSystem(new TestConsole(output));
        }

        [Fact]
        public void GetDirectoryPath_returns_full_path_for_directory()
        {
            string expectedDirectoryPath = PathHelper.SampleDirectoryPath("SampleSuccessfulTests");

            Assert.Equal(expectedDirectoryPath, _service.GetDirectoryPath(expectedDirectoryPath));


        }

        [Fact]
        public void GetDirectoryPath_returns_full_path_for_file()
        {
            string expectedDirectoryPath = PathHelper.SampleDirectoryPath("SampleSuccessfulTests");
            string filePath = PathHelper.SampleDirectoryPath("SampleSuccessfulTests/SampleSuccessfulTests.csproj");

            Assert.Equal(expectedDirectoryPath, _service.GetDirectoryPath(filePath));
        }

        [Fact]
        public void GetDirectoryPath_throws_if_directory_does_not_exist()
        {
            string path = PathHelper.SampleDirectoryPath("FAKE_DIRECTORY");

            Assert.Throws<DirectoryNotFoundException>(() => _service.GetDirectoryPath(path));
        }

        [Fact]
        public void GetDirectoryPath_throws_if_file_does_not_exist()
        {
            string path = PathHelper.SampleDirectoryPath("FAKE_DIRECTORY/Fake.csproj");

            Assert.Throws<DirectoryNotFoundException>(() => _service.GetDirectoryPath(path));
        }

        [Fact]
        public void GetDirectoryPath_with_file_that_does_not_exist_does_not_throw_if_throwIfNotFound_is_false()
        {
            string path = PathHelper.SampleDirectoryPath("FAKE_DIRECTORY/Fake.csproj");

            Assert.Null(_service.GetDirectoryPath(path, false));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetDirectoryPath_throws_if_path_is_null(string path)
        {
            Assert.Throws<ArgumentException>(() => _service.GetDirectoryPath(path));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetDirectoryPath_does_not_throw_if_path_is_null_and_throwIfNotFound_is_false(string path)
        {
            Assert.Null(_service.GetDirectoryPath(path, false));
        }

        [Fact]
        public void FindAllProjectFiles_returns_a_list_of_dotnet_projects()
        {
            List<FileInfo> projectFiles = _service.FindAllProjectFiles();

            Assert.Equal(12, projectFiles.Count);
        }

        [Fact]
        public void CreateConfigFile_creates_a_new_file()
        {
            string newConfigurationPath = PathHelper.SampleDirectoryPath($"temp/{Guid.NewGuid()}");

            if (!Directory.Exists(newConfigurationPath))
            {
                Directory.CreateDirectory(newConfigurationPath);
            }

            _service.WorkingDirectory = newConfigurationPath;

            FileInfo configFile = _service.CreateConfigFile();

            Assert.True(configFile.Exists, $"Config File does not exist: {configFile.FullName}");

            Directory.Delete(newConfigurationPath, true);
        }

        [Fact]
        public void CreateConfigFile_throws_if_config_file_already_exists()
        {
            string newConfigurationPath = PathHelper.SampleDirectoryPath($"temp/{Guid.NewGuid()}");

            if (!Directory.Exists(newConfigurationPath))
            {
                Directory.CreateDirectory(newConfigurationPath);
            }

            using (var stream = File.Create(Path.Combine(newConfigurationPath, "regi.json")))
            {
                stream.Close();
            }

            _service.WorkingDirectory = newConfigurationPath;

            Assert.Throws<InvalidOperationException>(() => _service.CreateConfigFile());

            Directory.Delete(newConfigurationPath, true);
        }
    }
}
