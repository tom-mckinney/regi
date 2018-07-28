using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regiment.Services;
using Regiment.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Regiment.Test.Services
{
    public class FileServiceTests
    {
        private IFileService _service;

        public FileServiceTests()
        {
            _service = new FileService(new TestCommandLineContext());
        }

        [Fact]
        public void FindAllProjectFiles_returns_a_list_of_dotnet_projects()
        {
            List<FileInfo> projectFiles = _service.FindAllProjectFiles();

            Assert.Equal(4, projectFiles.Count);
        }
    }
}
