using McMaster.Extensions.CommandLineUtils;
using Moq;
using Regiment.Models;
using Regiment.Services;
using Regiment.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regiment.Test.Services
{
    public class DotnetServiceTests
    {
        private readonly IDotnetService _service;

        public DotnetServiceTests(ITestOutputHelper testOutput)
        {
            _service = new DotnetService(new TestConsole(testOutput));
        }

        [Fact]
        public void TestProject_prints_all_output_when_verbose()
        {
            FileInfo testProject = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetFiles("SampleSuccessfulTests.csproj", SearchOption.AllDirectories)
                .First();

            DotnetProcess unitTest = _service.TestProject(testProject, true);

            Assert.Equal(DotnetTask.Test, unitTest.Task);
            Assert.Equal(DotnetResult.Success, unitTest.Result);
        }
    }
}
