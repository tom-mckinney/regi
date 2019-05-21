using McMaster.Extensions.CommandLineUtils.Abstractions;
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
    public class FileServiceTests
    {
        private IFileService _service;

        public FileServiceTests(ITestOutputHelper output)
        {
            _service = new FileService(new TestCommandLineContext(), new TestConsole(output));
        }

        [Fact]
        public void FindAllProjectFiles_returns_a_list_of_dotnet_projects()
        {
            List<FileInfo> projectFiles = _service.FindAllProjectFiles();

            Assert.Equal(6, projectFiles.Count);
        }

        [Fact]
        public void CreateConfigFile_creates_a_new_file()
        {
            string newConfigurationPath = PathHelper.SampleDirectoryPath("temp");

            if (!Directory.Exists(newConfigurationPath))
            {
                Directory.CreateDirectory(newConfigurationPath);
            }

            DirectoryUtility.SetTargetDirectory(newConfigurationPath);

            FileInfo configFile = _service.CreateConfigFile();

            Assert.True(configFile.Exists, $"Config File does not exist: {configFile.FullName}");
        }
    }
}
