using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class NodeServiceTests
    {
        private readonly IConsole _console;
        private readonly INodeService _service;

        private readonly Project _application;

        public NodeServiceTests(ITestOutputHelper testOutput)
        {
            _console = new TestConsole(testOutput);
            _service = new NodeService(_console);

            DirectoryInfo projectDir = new DirectoryInfo(Directory.GetCurrentDirectory())
                .GetDirectories("SampleNodeApp", SearchOption.AllDirectories)
                .First();

            _application = new Project("SampleNodeApp", projectDir
                .GetFiles("package.json", SearchOption.AllDirectories)
                .First()
                .FullName);
        }

        //[Fact]
        //public void RunProject_returns_failure_status_on_thrown_exception()
        //{
        //    throw new NotImplementedException();
        //    //AppProcess app = _service.RunProject(_applicationError);

        //    //app.Process.WaitForExit();

        //    //Assert.Equal(AppStatus.Failure, app.Status);
        //}

        [Fact]
        public void RunProject_starts_and_returns_process()
        {
            using (AppProcess app = _service.StartProject(_application, true))
            {
                Thread.Sleep(1000);

                Assert.Equal(AppTask.Start, app.Task);
                Assert.Equal(AppStatus.Running, app.Status);
            }
        }

        [Fact]
        public void RunProject_will_start_custom_port_if_specified()
        {
            using (AppProcess app = _service.StartProject(_application, true, 8080))
            {
                Thread.Sleep(1000);

                Assert.Equal(AppTask.Start, app.Task);
                Assert.Equal(AppStatus.Running, app.Status);
                Assert.Equal(8080, app.Port);
            }
        }

        [Theory]
        [InlineData(null, AppStatus.Failure)]
        [InlineData("passing", AppStatus.Success)]
        [InlineData("failing", AppStatus.Failure)]
        public void TestProject_will_return_test_for_path_pattern_and_expected_status(string pathPattern, AppStatus expectedStatus)
        {
            using (AppProcess test = _service.TestProject(_application, pathPattern, true))
            {
                Assert.Equal(AppTask.Test, test.Task);
                Assert.Equal(expectedStatus, test.Status);
            }
        }

        [Fact]
        public void InstallProject_returns_process()
        {
            using (AppProcess process = _service.InstallProject(_application, true))
            {
                Assert.Equal(AppTask.Install, process.Task);
                Assert.Equal(AppStatus.Success, process.Status);
                Assert.Null(process.Port);
            }
        }
    }
}
