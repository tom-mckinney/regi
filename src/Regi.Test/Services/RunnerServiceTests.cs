﻿using Moq;
using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using Regi.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    [Collection("FileSystem")]
    public class RunnerServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;
        private readonly Mock<IConfigurationService> _configurationServiceMock = new Mock<IConfigurationService>(MockBehavior.Strict);
        private readonly Mock<IDotnetService> _dotnetServiceMock = new Mock<IDotnetService>();
        private readonly Mock<INodeService> _nodeServiceMock = new Mock<INodeService>();
        private readonly Mock<IFrameworkServiceProvider> _frameworkServiceProviderMock = new Mock<IFrameworkServiceProvider>(MockBehavior.Loose);
        private readonly TestQueueService _queueService;
        private readonly Mock<INetworkingService> _networkingServiceMock = new Mock<INetworkingService>();
        private readonly Mock<IFileService> _fileServiceMock = new Mock<IFileService>();
        private readonly IRunnerService _runnerService;

        public RunnerServiceTests(ITestOutputHelper output)
        {
            _frameworkServiceProviderMock.Setup(m => m.GetFrameworkService(ProjectFramework.Dotnet))
                .Returns(_dotnetServiceMock.Object);
            _frameworkServiceProviderMock.Setup(m => m.GetFrameworkService(ProjectFramework.Node))
                .Returns(_nodeServiceMock.Object);

            _output = output;
            _console = new TestConsole(output);
            _queueService = new TestQueueService(_console);
            _runnerService = new RunnerService(
                new ProjectManager(_console, new Mock<ICleanupService>().Object),
                _configurationServiceMock.Object,
                _frameworkServiceProviderMock.Object,
                _queueService,
                _networkingServiceMock.Object,
                _fileServiceMock.Object,
                _console);
        }

        [Fact]
        public void Start_returns_a_project_for_every_app_in_startup_config()
        {
            var configuration = SampleProjects.ConfigurationGood;

            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(configuration)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, s, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, s, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var processes = _runnerService.Start(TestOptions.Create());

            Assert.Equal(configuration.Apps.Count, processes.Count);
            Assert.Single(processes, p => p.Port == 9080);
            Assert.Equal(processes.Count(p => p.Port.HasValue), _queueService.ActivePorts.Distinct().Count());

            _configurationServiceMock.Verify();
            _dotnetServiceMock.Verify(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()), Times.Exactly(configuration.Apps.Count(a => a.Framework == ProjectFramework.Dotnet)));
            _nodeServiceMock.Verify(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()), Times.Exactly(configuration.Apps.Count(a => a.Framework == ProjectFramework.Node)));
        }

        [Fact]
        public void Start_sets_port_and_url_variables_for_every_app_and_waits_on_port()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, s, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, s, b) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var projects = _runnerService.Start(TestOptions.Create());

            foreach (var p in projects)
            {
                if (p.Port.HasValue)
                {
                    _dotnetServiceMock.Verify(m => m.StartProject(It.IsAny<Project>(),
                         It.IsAny<string>(),
                        It.Is<RegiOptions>(o => o.VariableList.ContainsKey($"{p.Name}_PORT".ToUnderscoreCase()) &&
                        o.VariableList.ContainsKey($"{p.Name}_URL".ToUnderscoreCase()))));

                    Assert.Contains(p.Port.Value, _queueService.ActivePorts);
                }
            }
        }

        [Fact]
        public void Start_adds_serial_projects_to_serial_queue_and_all_others_to_parallel_queue()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, s, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, s, b) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var projects = _runnerService.Start(TestOptions.Create());

            Assert.Equal(2, _queueService.ParallelActions.Count);
            Assert.Single(_queueService.SerialActions);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Start_creates_an_AppProcess_for_every_path_in_Project(int pathCount)
        {
            var appCollection = SampleProjects.AppCollection;
            appCollection.Paths = appCollection.Paths.Take(pathCount).ToList();
            var config = new StartupConfig
            {
                Apps = new List<Project> { appCollection }
            };

            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(config)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, path, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Throws(new System.Exception("Should not be called"));

            var projects = _runnerService.Start(TestOptions.Create());

            Assert.Equal(pathCount, _queueService.ParallelActions.Count);
            _dotnetServiceMock.Verify(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()), Times.Exactly(pathCount));

            var project = Assert.Single(projects);
            Assert.Equal(pathCount, project.Processes.Count);
        }

        [Fact]
        public void Test_returns_a_project_for_every_test_in_startup_config()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, d, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var processes = _runnerService.Test(TestOptions.Create());

            Assert.Equal(SampleProjects.ConfigurationGood.Tests.Count, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public void Test_also_starts_every_requirement_for_each_test()
        {
            TestQueueService dependencyQueueService = new TestQueueService(_console);

            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(dependencyQueueService)
                .Verifiable();
            _nodeServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Start, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.StartProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Start, AppStatus.Success))
                .Verifiable();

            var projects = _runnerService.Test(TestOptions.Create());

            Assert.Equal(SampleProjects.ConfigurationDefault.Tests.Count, projects.Count);

            foreach (var project in projects)
            {
                var process = Assert.Single(project.Processes);
                Assert.Equal(AppTask.Test, process.Task);
                if (project.Requires?.Count > 0)
                {
                    Assert.Equal(project.Requires.Count, project.RequiredProjects.Count);
                    foreach (var requirement in project.Requires)
                    {
                        var requiredApp = Assert.Single(project.RequiredProjects, p => p.Name == requirement);
                        var requiredAppProcess = Assert.Single(requiredApp.Processes);
                        Assert.Equal(AppTask.Start, requiredAppProcess.Task);

                        Assert.Contains(requiredApp.Port.Value, _queueService.ActivePorts); // Waited on during InternalStart
                        Assert.Contains(requiredApp.Port.Value, dependencyQueueService.ActivePorts); // Waited on after starting Test dependencies
                    }
                }
            }

            _frameworkServiceProviderMock.Verify();
            _dotnetServiceMock.VerifyAll();
            _nodeServiceMock.VerifyAll();
        }

        [Fact]
        public void Test_requirements_with_multiple_paths_will_start_every_path()
        {
            var dependencyQueue = new TestQueueService(_console);
            var appCollection = SampleProjects.AppCollection;
            var testProject = SampleProjects.XunitTests;
            testProject.Requires = new List<string> { SampleProjects.AppCollection.Name };

            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(new StartupConfig
                {
                    Apps = new List<Project> { appCollection },
                    Tests = new List<Project> { testProject }
                })
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();
            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(dependencyQueue)
                .Verifiable();

            foreach (var path in SampleProjects.AppCollection.Paths)
            {
                _dotnetServiceMock.Setup(m => m.StartProject(appCollection, It.IsAny<string>(), It.IsAny<RegiOptions>()))
                    .Returns(new AppProcess(new Process(), AppTask.Start, AppStatus.Running))
                    .Verifiable();
            }

            var projects = _runnerService.Test(TestOptions.Create());

            var actualTestProject = Assert.Single(projects);
            Assert.Same(testProject, actualTestProject);
            Assert.Single(testProject.Processes);
            var requiredProject = Assert.Single(testProject.RequiredProjects);
            Assert.Same(appCollection, requiredProject);
            Assert.Equal(3, requiredProject.Processes.Count);
            Assert.Equal(3, dependencyQueue.ParallelActions.Count);

            _configurationServiceMock.Verify();
            _dotnetServiceMock.Verify();
            _frameworkServiceProviderMock.Verify();
        }

        [Theory]
        [InlineData(ProjectType.Unit, 1)]
        [InlineData(ProjectType.Integration, 1)]
        public void Test_will_only_run_tests_on_type_specified(ProjectType type, int expectedCount)
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.Test(new RegiOptions { Type = type });

            Assert.Equal(expectedCount, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public void Test_adds_serial_projects_to_serial_queue_and_all_others_to_parallel_queue()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.TestProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns<Project, string, RegiOptions>((p, d, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var processes = _runnerService.Test(TestOptions.Create());

            Assert.Single(_queueService.ParallelActions);
            Assert.Single(_queueService.SerialActions);
        }

        [Fact]
        public void Install_returns_a_process_for_every_app_and_test_project()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.Install(TestOptions.Create());

            int appsCount = SampleProjects.ConfigurationGood.Apps.Count;
            int testsCount = SampleProjects.ConfigurationGood.Tests.Count;
            Assert.Equal(appsCount + testsCount, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public void Install_sets_package_repo_to_value_configured_startup_file()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationGood)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var processes = _runnerService.Install(TestOptions.Create());

            var dotnetApp = Assert.Single(processes, p => p.Framework == ProjectFramework.Dotnet && p.Type == ProjectType.Web && p.Port.HasValue);
            var nodeApp = Assert.Single(processes, p => p.Framework == ProjectFramework.Node && p.Type == ProjectType.Web && p.Port.HasValue);

            Assert.Equal("http://nuget.org/api", dotnetApp.Source);
            Assert.Equal("http://npmjs.org", nodeApp.Source);
        }

        [Fact]
        public void Install_will_also_install_for_any_required_dependencies()
        {
            TestQueueService dependencyQueueService = new TestQueueService(_console);

            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(dependencyQueueService)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var options = TestOptions.Create();

            options.Name = "IntegrationTests";

            var outputProjects = _runnerService.Install(options);

            var integrationTestProject = Assert.Single(outputProjects);

            Assert.Equal(2, integrationTestProject.RequiredProjects.Count);

            var dotnetApp = Assert.Single(integrationTestProject.RequiredProjects, p => p.Framework == ProjectFramework.Dotnet);
            //Assert.Equal(AppTask.Install, dotnetApp.Process.Task);
            //Assert.Equal(AppStatus.Success, dotnetApp.Process.Status);
            Assert.Equal(AppStatus.Success, dotnetApp.OutputStatus);

            var nodeApp = Assert.Single(integrationTestProject.RequiredProjects, p => p.Framework == ProjectFramework.Node);
            //Assert.Equal(AppTask.Install, nodeApp.Process.Task);
            Assert.Equal(AppStatus.Success, nodeApp.OutputStatus);

            _configurationServiceMock.Verify();
            _frameworkServiceProviderMock.Verify();
            _dotnetServiceMock.Verify();
            _nodeServiceMock.Verify();
        }

        [Fact]
        public void Install_will_not_install_a_dependency_if_it_is_also_a_top_level_filtered_project()
        {
            TestQueueService dependencyQueueService = new TestQueueService(_console);

            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(SampleProjects.ConfigurationDefault)
                .Verifiable();
            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(dependencyQueueService)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var outputProjects = _runnerService.Install(TestOptions.Create());

            var allAppsAndTests = SampleProjects.ConfigurationDefault.Apps
                .Concat(SampleProjects.ConfigurationDefault.Tests)
                .ToList();

            Assert.Equal(allAppsAndTests.Count, outputProjects.Count);

            foreach (var project in outputProjects)
            {
                Assert.Empty(project.RequiredProjects);
            }

            _configurationServiceMock.Verify();
            _frameworkServiceProviderMock.Verify();
            _dotnetServiceMock.Verify();
            _nodeServiceMock.Verify();
        }

        [Fact]
        public void Install_will_not_install_a_dependency_if_it_has_already_been_installed_as_another_project_dependency()
        {
            var app = SampleProjects.Backend;

            var test1 = SampleProjects.XunitTests;
            test1.Name = "Test1";
            test1.Requires = new List<string> { app.Name };

            var test2 = SampleProjects.XunitTests;
            test2.Name = "Test2";
            test2.Requires = new List<string> { app.Name };

            var options = TestOptions.Create();
            options.NoParallel = true;
            options.Type = test1.Type;

            _configurationServiceMock.Setup(m => m.GetConfiguration())
                .Returns(new StartupConfig
                {
                    Apps = new List<Project> { app },
                    Tests = new List<Project> { test1, test2 }
                })
                .Verifiable();
            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(() => new TestQueueService(_console))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()))
                .Returns(() => new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var outputProjects = _runnerService.Install(options);

            Assert.Equal(2, outputProjects.Count);
            Assert.Single(outputProjects, p => p.RequiredProjects.Count > 0);

            _configurationServiceMock.Verify();
            _frameworkServiceProviderMock.Verify();
            _dotnetServiceMock.Verify(m => m.InstallProject(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<RegiOptions>()), Times.Exactly(3));
        }

        [Fact]
        public void Initialize_returns_a_single_process_and_creates_a_config_file()
        {
            _fileServiceMock.Setup(m => m.CreateConfigFile())
                .Returns(new FileInfo("regi.json"))
                .Verifiable();

            DirectoryUtility.SetTargetDirectory(PathHelper.SampleDirectoryPath("ConfigurationNew"));

            _runnerService.Initialize(TestOptions.Create());

            _fileServiceMock.VerifyAll();
        }

        [Fact]
        public void Kill_calls_service_to_kill_for_each_ProjectFramework()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
               .Returns(SampleProjects.ConfigurationGood)
               .Verifiable();
            _nodeServiceMock.Setup(m => m.KillProcesses(It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.KillProcesses(It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();

            var options = TestOptions.Create();

            _runnerService.Kill(options);

            _nodeServiceMock.Verify(m => m.KillProcesses(options), Times.Once);
            _dotnetServiceMock.Verify(m => m.KillProcesses(options), Times.Once);
        }

        [Fact]
        public void Kill_will_call_kill_for_every_type_of_ProjectFramework_if_there_is_no_startup_config()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
               .Throws(new FileNotFoundException())
               .Verifiable();
            _frameworkServiceProviderMock.Setup(m => m.GetAllProjectFrameworkTypes())
                .Returns(new List<ProjectFramework> { ProjectFramework.Dotnet, ProjectFramework.Node })
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.KillProcesses(It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.KillProcesses(It.IsAny<RegiOptions>()))
                .Returns(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();

            var options = TestOptions.Create();

            _runnerService.Kill(options);

            _configurationServiceMock.Verify();
            _frameworkServiceProviderMock.Verify();
            _nodeServiceMock.Verify(m => m.KillProcesses(options), Times.Once);
            _dotnetServiceMock.Verify(m => m.KillProcesses(options), Times.Once);
        }

        [Fact]
        public void List_prints_all_apps_and_tests()
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
               .Returns(SampleProjects.ConfigurationGood)
               .Verifiable();

            var output = _runnerService.List(TestOptions.Create());

            Assert.Equal(SampleProjects.ConfigurationGood.Apps.Count, output.Apps.Count);
            Assert.Equal(SampleProjects.ConfigurationGood.Tests.Count, output.Tests.Count);

            Assert.Contains("Apps:", _console.LogOutput);
            Assert.Contains("Tests:", _console.LogOutput);
        }

        [Theory]
        [InlineData("node", 1, 0)]
        [InlineData("test", 0, 2)]
        [InlineData("SampleApp1", 1, 0)]
        public void List_prints_only_apps_or_tests_that_match_name_if_specified(string name, int appCount, int testCount)
        {
            _configurationServiceMock.Setup(m => m.GetConfiguration())
               .Returns(SampleProjects.ConfigurationGood)
               .Verifiable();

            var output = _runnerService.List(new RegiOptions { Name = name });

            Assert.Equal(appCount, output.Apps.Count);
            Assert.Equal(testCount, output.Tests.Count);

            if (appCount <= 0)
                Assert.DoesNotContain("Apps:", _console.LogOutput);
            if (testCount <= 0)
                Assert.DoesNotContain("Tests:", _console.LogOutput);
        }
    }
}
