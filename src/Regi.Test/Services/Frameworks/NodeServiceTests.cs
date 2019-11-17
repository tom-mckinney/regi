﻿using McMaster.Extensions.CommandLineUtils;
using Moq;
using Regi.Models;
using Regi.Services;
using Regi.Services.Frameworks;
using Regi.Test.Helpers;
using Regi.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services.Frameworks
{
    [Collection(TestCollections.NoParallel)]
    public class NodeServiceTests : TestBase
    {
        private readonly IConsole _console;
        private readonly Mock<IRuntimeInfo> _runtimeInfoMock = new Mock<IRuntimeInfo>();
        private readonly INodeService _service;

        private readonly Project _application = SampleProjects.SimpleNodeApp;

        public NodeServiceTests(ITestOutputHelper testOutput)
        {
            _console = new TestConsole(testOutput);
            _service = new NodeService(_console, new PlatformService(_runtimeInfoMock.Object, _fileSystem, _console));
        }

        [Fact]
        public async Task RunProject_starts_and_returns_process()
        {
            AppProcess app = await _service.StartProject(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Thread.Sleep(1000);

            Assert.Equal(AppTask.Start, app.Task);
            Assert.Equal(AppStatus.Running, app.Status);

            CleanupApp(app);
        }

        [Fact]
        public async Task RunProject_will_start_custom_port_if_specified()
        {
            int expectedPort = new Random().Next(1000, 9999);

            _application.Port = expectedPort;

            AppProcess app = await _service.StartProject(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Thread.Sleep(1000);

            Assert.Equal(AppTask.Start, app.Task);
            Assert.Equal(AppStatus.Running, app.Status);
            Assert.Equal(expectedPort, app.Port);

            CleanupApp(app);
        }

        [Fact]
        public async Task RunProject_will_add_all_variables_passed_to_process()
        {
            _application.Port = 8080;

            EnvironmentVariableDictionary varList = new EnvironmentVariableDictionary
            {
                { "foo", "bar" }
            };

            AppProcess app = await _service.StartProject(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(varList), CancellationToken.None);

            Thread.Sleep(500);

            Assert.Equal(AppTask.Start, app.Task);
            Assert.Equal(AppStatus.Running, app.Status);
            Assert.Equal(8080, app.Port);
            Assert.True(app.Process.StartInfo.EnvironmentVariables.ContainsKey("foo"), "Environment variable \"foo\" has not been set.");
            Assert.Equal("bar", app.Process.StartInfo.EnvironmentVariables["foo"]);

            CleanupApp(app);
        }

        [Theory]
        [InlineData(null, AppStatus.Failure)]
        [InlineData("passing", AppStatus.Success)]
        [InlineData("failing", AppStatus.Failure)]
        public async Task TestProject_will_return_test_for_path_pattern_and_expected_status(string pathPattern, AppStatus expectedStatus)
        {
            AppProcess test = await _service.TestProject(_application, _application.GetAppDirectoryPaths(_fileSystem)
                [0], TestOptions.Create(null, pathPattern), CancellationToken.None);

            Assert.Equal(AppTask.Test, test.Task);
            Assert.Equal(expectedStatus, test.Status);
        }

        [Fact]
        public async Task TestProject_will_add_all_variables_passed_to_process()
        {
            _application.Port = 8080;

            EnvironmentVariableDictionary varList = new EnvironmentVariableDictionary
            {
                { "foo", "bar" }
            };

            AppProcess testProcess = await _service.TestProject(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(varList), CancellationToken.None);

            Assert.Equal(AppTask.Test, testProcess.Task);
            Assert.Equal(AppStatus.Failure, testProcess.Status);
            Assert.Equal(8080, testProcess.Port);
            Assert.True(testProcess.Process.StartInfo.EnvironmentVariables.ContainsKey("foo"), "Environment variable \"foo\" has not been set.");
            Assert.Equal("bar", testProcess.Process.StartInfo.EnvironmentVariables["foo"]);
        }

        [Fact]
        public async Task InstallProject_returns_process()
        {
            AppProcess process = await _service.InstallProject(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppTask.Install, process.Task);
            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Equal(_application.Port, process.Port);
        }

        [Fact]
        public async Task InstallProjects_sets_registry_url_if_specified()
        {
            string source = "https://artifactory.org/npm";
            _application.Source = source;

            AppProcess process = await _service.InstallProject(_application, _application.GetAppDirectoryPaths(_fileSystem)[0], TestOptions.Create(), CancellationToken.None);

            Assert.Contains($"--registry {source}", process.Process.StartInfo.Arguments, StringComparison.InvariantCulture);
        }

        [Theory]
        [InlineData(true, "taskkill", "/F /IM node.exe")]
        [InlineData(false, "killall", "node")]
        public async Task KillProcesses_kills_all_node_processes(bool isWindows, string expectedFileName, string expectedArguments)
        {
            _runtimeInfoMock.Setup(m => m.IsWindows).Returns(isWindows).Verifiable();

            AppProcess process = await _service.KillProcesses(TestOptions.Create(), CancellationToken.None);

            Assert.Equal(AppStatus.Success, process.Status);
            Assert.Equal(AppTask.Kill, process.Task);

            var startInfo = process.Process.StartInfo;
            Assert.Equal(expectedFileName, startInfo.FileName);
            Assert.Equal(expectedArguments, startInfo.Arguments);

            _runtimeInfoMock.Verify();
        }
    }
}
