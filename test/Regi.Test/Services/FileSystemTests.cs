using Regi.Abstractions;
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
    public class FileSystemTests
    {
        private readonly IFileSystem _service;

        public FileSystemTests(ITestOutputHelper output)
        {
            _service = new FileSystem(new TestConsole(output))
            {
                WorkingDirectory = PathHelper.SampleProjectsRootPath
            };
        }

        [Fact]
        public void GetDirectoryPath_returns_full_path_for_directory()
        {
            string expectedDirectoryPath = PathHelper.GetSampleProjectPath("SampleSuccessfulTests");

            Assert.Equal(expectedDirectoryPath, _service.GetDirectoryPath(expectedDirectoryPath));
        }

        [Fact]
        public void GetDirectoryPath_returns_full_path_for_file()
        {
            string expectedDirectoryPath = PathHelper.GetSampleProjectPath("SampleSuccessfulTests");
            string filePath = PathHelper.GetSampleProjectPath("SampleSuccessfulTests/SampleSuccessfulTests.csproj");

            Assert.Equal(expectedDirectoryPath, _service.GetDirectoryPath(filePath));
        }

        [Fact]
        public void GetDirectoryPath_throws_if_directory_does_not_exist()
        {
            string path = PathHelper.GetSampleProjectPath("FAKE_DIRECTORY");

            Assert.Throws<DirectoryNotFoundException>(() => _service.GetDirectoryPath(path));
        }

        [Fact]
        public void GetDirectoryPath_throws_if_file_does_not_exist()
        {
            string path = PathHelper.GetSampleProjectPath("FAKE_DIRECTORY/Fake.csproj");

            Assert.Throws<DirectoryNotFoundException>(() => _service.GetDirectoryPath(path));
        }

        [Fact]
        public void GetDirectoryPath_with_file_that_does_not_exist_does_not_throw_if_throwIfNotFound_is_false()
        {
            string path = PathHelper.GetSampleProjectPath("FAKE_DIRECTORY/Fake.csproj");

            Assert.Null(_service.GetDirectoryPath(path, false));
        }

        [Fact]
        public void GetRelativePath_success()
        {
            string absolutePath = PathHelper.GetSampleProjectPath("ClassLib");
            string expectedRelativePath = "./ClassLib";

            string actualRelativePath = _service.GetRelativePath(absolutePath);

            Assert.Equal(expectedRelativePath, actualRelativePath);
        }

        [Fact]
        public void GetRelativePath_one_folder_down()
        {
            string absolutePath = PathHelper.GetSampleProjectPath("ClassLib");
            _service.WorkingDirectory = new DirectoryInfo(absolutePath).Parent.FullName;
            string expectedRelativePath = Path.GetRelativePath(_service.WorkingDirectory, absolutePath).Replace('\\', '/');

            string actualRelativePath = _service.GetRelativePath(absolutePath);

            Assert.Equal("./ClassLib", actualRelativePath);
            Assert.NotEqual(expectedRelativePath, actualRelativePath);
        }

        [Fact]
        public void GetRelativePath_success_same_folder()
        {
            string absolutePath = PathHelper.GetSampleProjectPath("ClassLib");
            _service.WorkingDirectory = new DirectoryInfo(absolutePath).FullName;
            string expectedRelativePath = Path.GetRelativePath(_service.WorkingDirectory, absolutePath).Replace('\\', '/');

            string actualRelativePath = _service.GetRelativePath(absolutePath);

            Assert.Equal("./", actualRelativePath);
            Assert.NotEqual(expectedRelativePath, actualRelativePath);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetRelativePath_null(string path)
        {
            Assert.Throws<ArgumentNullException>(() => _service.GetRelativePath(path));
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
        public void GetChildDirectories_success()
        {
            var directory = new DirectoryInfo(PathHelper.SampleProjectsRootPath);
            var expectedChildDirectories = directory.GetDirectories();

            var actualChildDirectories = _service.GetChildDirectories(directory);

            for (int i = 0; i < actualChildDirectories.Count(); i++)
            {
                var expected = expectedChildDirectories[i];
                var actual = actualChildDirectories.ElementAt(i);

                Assert.Equal(expected.FullName, actual.FullName);
            }
        }

        [Theory]
        [InlineData("wumbo")]
        [InlineData(null)]
        public void GetChildDirectories_throws_if_directory_does_not_exist(string path)
        {
            DirectoryInfo directory = path == null ? null : new DirectoryInfo(path);
            Assert.Throws<ArgumentException>(() => _service.GetChildDirectories(directory));
        }

        [Fact]
        public void GetAllChildren_success()
        {
            var directory = new DirectoryInfo(PathHelper.SampleProjectsRootPath);
            var expectedChildren = directory.GetFileSystemInfos();

            var actualChildDirectories = _service.GetAllChildren(directory);

            for (int i = 0; i < actualChildDirectories.Count; i++)
            {
                var expected = expectedChildren[i];
                var actual = actualChildDirectories.ElementAt(i).Value;

                Assert.Equal(expected.FullName, actual.FullName);
            }
        }

        [Theory]
        [InlineData("wumbo")]
        [InlineData(null)]
        public void GetAllChildren_throws_if_directory_does_not_exist(string path)
        {
            DirectoryInfo directory = path == null ? null : new DirectoryInfo(path);
            Assert.Throws<ArgumentException>(() => _service.GetChildDirectories(directory));
        }

        [Fact]
        public void FindAllProjectFiles_returns_a_list_of_dotnet_projects()
        {
            List<FileInfo> projectFiles = _service.FindAllProjectFiles();

            Assert.Equal(13, projectFiles.Count);
        }

        [Fact]
        public async Task CreateConfigFile_creates_a_new_file()
        {
            string newConfigurationPath = PathHelper.GetSampleProjectPath($"temp/{Guid.NewGuid()}");

            if (!Directory.Exists(newConfigurationPath))
            {
                Directory.CreateDirectory(newConfigurationPath);
            }

            _service.WorkingDirectory = newConfigurationPath;

            FileInfo configFile = await _service.CreateConfigFileAsync(SampleProjects.ConfigurationDefault);

            Assert.True(configFile.Exists, $"Config File does not exist: {configFile.FullName}");

            Directory.Delete(newConfigurationPath, true);
        }

        [Fact]
        public async Task CreateConfigFile_throws_if_config_file_already_exists()
        {
            string newConfigurationPath = PathHelper.GetSampleProjectPath($"temp/{Guid.NewGuid()}");

            if (!Directory.Exists(newConfigurationPath))
            {
                Directory.CreateDirectory(newConfigurationPath);
            }

            using (var stream = File.Create(Path.Combine(newConfigurationPath, "regi.json")))
            {
                stream.Close();
            }

            _service.WorkingDirectory = newConfigurationPath;

            await Assert.ThrowsAsync<RegiException>(() => _service.CreateConfigFileAsync(SampleProjects.ConfigurationDefault).AsTask());

            Directory.Delete(newConfigurationPath, true);
        }
    }
}
