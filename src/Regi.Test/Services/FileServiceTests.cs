using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Regi.Test.Services
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

            Assert.Equal(5, projectFiles.Count);
        }
    }
}
