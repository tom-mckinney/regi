using Moq;
using Regi.Extensions;
using Regi.Frameworks;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    [Collection(TestCollections.NoParallel)]
    public class RunnerServiceTests
    {
        private readonly TestConsole _console;
        private readonly Mock<IDotnet> _dotnetServiceMock = new Mock<IDotnet>();
        private readonly Mock<INode> _nodeServiceMock = new Mock<INode>();
        private readonly Mock<IFrameworkServiceProvider> _frameworkServiceProviderMock = new Mock<IFrameworkServiceProvider>(MockBehavior.Loose);
        private readonly TestQueueService _queueService;
        private readonly Mock<INetworkingService> _networkingServiceMock = new Mock<INetworkingService>();
        private readonly Mock<IPlatformService> _platformServiceMock = new Mock<IPlatformService>();
        private readonly IRunnerService _runnerService;
        private readonly TestFileSystem _fileSystem = new TestFileSystem();

        public RunnerServiceTests(ITestOutputHelper output)
        {
            _frameworkServiceProviderMock.Setup(m => m.GetFrameworkService(ProjectFramework.Dotnet))
                .Returns(_dotnetServiceMock.Object);
            _frameworkServiceProviderMock.Setup(m => m.GetFrameworkService(ProjectFramework.Node))
                .Returns(_nodeServiceMock.Object);

            _console = new TestConsole(output);
            _queueService = new TestQueueService(_console);
            _runnerService = new RunnerService(
                new ProjectManager(_console, new Mock<ICleanupService>().Object),
                _frameworkServiceProviderMock.Object,
                _queueService,
                _networkingServiceMock.Object,
                _platformServiceMock.Object,
                _fileSystem,
                _console);
        }

        [Fact]
        public async Task Start_returns_a_project_for_every_app_in_startup_config()
        {
            var configuration = SampleProjects.ConfigurationGood;

            _dotnetServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string s, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                //.ReturnsAsync<Project, string, RegiOptions, CancellationToken>((p, s, o, c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string s, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                //.ReturnsAsync<Project, string, RegiOptions, CancellationToken>((p, s, o, c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var processes = await _runnerService.StartAsync(configuration.Projects, TestOptions.Create(), CancellationToken.None);

            Assert.Equal(configuration.Projects.Count, processes.Count);
            Assert.Single(processes, p => p.Port == 9080);
            Assert.Equal(processes.Count(p => p.Port.HasValue), _queueService.ActivePorts.Distinct().Count());

            _dotnetServiceMock.Verify(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(configuration.Projects.Count(a => a.Framework == ProjectFramework.Dotnet)));
            _nodeServiceMock.Verify(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(configuration.Projects.Count(a => a.Framework == ProjectFramework.Node)));
        }

        [Fact]
        public async Task Start_sets_port_and_url_variables_for_every_app_and_waits_on_port()
        {
            _dotnetServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string s, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                //.Returns<Project, string, RegiOptions>((p, s, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string s, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                //.Returns<Project, string, RegiOptions>((p, s, b) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var projects = await _runnerService.StartAsync(SampleProjects.ConfigurationGood.Projects, TestOptions.Create(), CancellationToken.None);

            foreach (var p in projects)
            {
                if (p.Port.HasValue)
                {
                    _dotnetServiceMock.Verify(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()));

                    Assert.Contains(p.Port.Value, _queueService.ActivePorts);
                }
            }
        }

        [Fact]
        public async Task Start_adds_serial_projects_to_serial_queue_and_all_others_to_async_queue()
        {
            _dotnetServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string s, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                //.Returns<Project, string, RegiOptions>((p, s, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string s, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                //.Returns<Project, string, RegiOptions>((p, s, b) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var projects = await _runnerService.StartAsync(SampleProjects.ConfigurationGood.Projects.WhereApp().ToList(), TestOptions.Create(), CancellationToken.None);

            Assert.Equal(2, _queueService.AsyncActions.Count);
            Assert.Single(_queueService.SerialActions);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task Start_creates_an_AppProcess_for_every_path_in_Project(int pathCount)
        {
            var appCollection = SampleProjects.AppCollection;
            appCollection.Paths = appCollection.Paths.Take(pathCount).ToList();
            var config = new RegiConfig
            {
                Projects = new List<Project> { appCollection }
            };

            _dotnetServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string s, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                //.Returns<Project, string, RegiOptions>((p, path, o) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .Throws(new System.Exception("Should not be called"));

            var projects = await _runnerService.StartAsync(config.Projects, TestOptions.Create(), CancellationToken.None);

            Assert.Equal(pathCount, _queueService.AsyncActions.Count);
            _dotnetServiceMock.Verify(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(pathCount));

            var project = Assert.Single(projects);
            Assert.Equal(pathCount, project.Processes.Count);
        }

        [Fact]
        public async Task Start_runs_simple_script_before_starting_if_they_are_specified()
        {
            var configuration = SampleProjects.ConfigurationGood;

            configuration.Projects[0].Scripts = new CommandDictionary<object>
            {
                { "start", new List<object> { "foo bar -v" } }
            };

            _platformServiceMock.Setup(m => m.RunAnonymousScript("foo bar -v", It.IsAny<CommandOptions>(), null))
                .Verifiable();

            var processes = await _runnerService.StartAsync(configuration.Projects, TestOptions.Create(), CancellationToken.None);

            _platformServiceMock.Verify();
        }

        [Fact]
        public async Task Start_runs_app_script_in_working_directory_before_starting_if_they_are_specified()
        {
            string workingDirectory = PathHelper.GetSampleProjectPath("ConfigurationGood");

            var configuration = SampleProjects.ConfigurationGood;

            configuration.Projects[0].Scripts = new CommandDictionary<object>
            {
                { "start", new List<object> { new AppScript("foo bar -v", workingDirectory) } }
            };

            _platformServiceMock.Setup(m => m.RunAnonymousScript("foo bar -v", It.IsAny<CommandOptions>(), workingDirectory))
                .Verifiable();

            var processes = await _runnerService.StartAsync(configuration.Projects, TestOptions.Create(), CancellationToken.None);

            _platformServiceMock.Verify();
        }

        [Fact]
        public async Task Start_does_not_run_script_if_it_is_not_string_or_app_script()
        {
            var configuration = SampleProjects.ConfigurationGood;

            configuration.Projects[0].Scripts = new CommandDictionary<object>
            {
                { "start", new List<object> { new { Foo = "Bar" } } }
            };

            _platformServiceMock.Setup(m => m.RunAnonymousScript(It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<string>()))
                .Throws(new Exception("This should not be called"));

            var processes = await _runnerService.StartAsync(configuration.Projects, TestOptions.Create(), CancellationToken.None);

            _platformServiceMock.Verify();
        }

        [Fact]
        public async Task Test_returns_a_project_for_every_test_in_startup_config()
        {
            _dotnetServiceMock.Setup(m => m.Test(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string d, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();

            var testsProjects = SampleProjects.ConfigurationGood.Projects.WhereTest().ToList();

            var processes = await _runnerService.TestAsync(testsProjects, TestOptions.Create(), CancellationToken.None);

            Assert.Equal(testsProjects.Count, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public async Task Test_also_starts_every_requirement_for_each_test()
        {
            TestQueueService dependencyQueueService = new TestQueueService(_console);

            var tests = SampleProjects.ConfigurationDefault.Projects;

            ProjectManager.LinkProjectRequirements(tests, TestOptions.Create(), SampleProjects.ConfigurationDefault);

            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(dependencyQueueService)
                .Verifiable();

            _nodeServiceMock.Setup(m => m.Test(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Start, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.Test(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.Start(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Start, AppStatus.Success))
                .Verifiable();

            var projects = await _runnerService.TestAsync(tests, TestOptions.Create(), CancellationToken.None);

            Assert.Equal(tests.Count, projects.Count);

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
        public async Task Test_requirements_with_multiple_paths_will_start_every_path()
        {
            var dependencyQueue = new TestQueueService(_console);
            var appCollection = SampleProjects.AppCollection;
            var testProject = SampleProjects.XunitTests;
            testProject.Requires = new List<string> { appCollection.Name };
            testProject.RequiredProjects.Add(appCollection);

            _dotnetServiceMock.Setup(m => m.Test(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Test, AppStatus.Success))
                .Verifiable();
            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(dependencyQueue)
                .Verifiable();

            foreach (var path in SampleProjects.AppCollection.Paths)
            {
                _dotnetServiceMock.Setup(m => m.Start(appCollection, It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AppProcess(new Process(), AppTask.Start, AppStatus.Running))
                    .Verifiable();
            }

            var projects = await _runnerService.TestAsync(new List<Project> { testProject }, TestOptions.Create(), CancellationToken.None);

            var actualTestProject = Assert.Single(projects);
            Assert.Same(testProject, actualTestProject);
            Assert.Single(testProject.Processes);
            var requiredProject = Assert.Single(testProject.RequiredProjects);
            Assert.Same(appCollection, requiredProject);
            Assert.Equal(3, requiredProject.Processes.Count);
            Assert.Equal(3, dependencyQueue.AsyncActions.Count);

            _dotnetServiceMock.Verify();
            _frameworkServiceProviderMock.Verify();
        }

        [Fact]
        public async Task Test_adds_serial_projects_to_serial_queue_and_all_others_to_async_queue()
        {
            _dotnetServiceMock.Setup(m => m.Test(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Project p, string d, CommandOptions o, CancellationToken c) => new AppProcess(new Process(), AppTask.Start, AppStatus.Success, p?.Port))
                .Verifiable();
            
            var testsProjects = SampleProjects.ConfigurationGood.Projects.WhereTest().ToList();

            var processes = await _runnerService.TestAsync(testsProjects, TestOptions.Create(), CancellationToken.None);

            Assert.Single(_queueService.AsyncActions);
            Assert.Single(_queueService.SerialActions);
        }

        [Fact]
        public async Task Install_returns_a_process_for_every_app_and_test_project()
        {
            _dotnetServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var projects = SampleProjects.ConfigurationGood.Projects
                .ToList();

            var processes = await _runnerService.InstallAsync(projects, TestOptions.Create(), CancellationToken.None);

            int appsCount = SampleProjects.ConfigurationGood.Projects.Count;
            Assert.Equal(appsCount, processes.Count);

            _dotnetServiceMock.VerifyAll();
        }

        [Fact]
        public async Task Install_sets_package_repo_to_value_configured_startup_file()
        {
            var projects = SampleProjects.ConfigurationGood.Projects;

            ProjectManager.LinkProjectRequirements(projects, TestOptions.Create(), SampleProjects.ConfigurationGood);

            _dotnetServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var processes = await _runnerService.InstallAsync(projects, TestOptions.Create(), CancellationToken.None);

            var dotnetApp = Assert.Single(processes, p => p.Framework == ProjectFramework.Dotnet && p.Roles.Contains(ProjectRole.App) && p.Port.HasValue);
            var nodeApp = Assert.Single(processes, p => p.Framework == ProjectFramework.Node && p.Roles.Contains(ProjectRole.App) && p.Port.HasValue);

            Assert.Equal("http://nuget.org/api", dotnetApp.Source);
            Assert.Equal("http://npmjs.org", nodeApp.Source);
        }

        [Fact]
        public async Task Install_will_also_install_for_any_required_dependencies()
        {
            TestQueueService dependencyQueueService = new TestQueueService(_console);

            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(dependencyQueueService)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var options = TestOptions.Create();

            var projects = new List<Project> { SampleProjects.IntegrationTests };

            ProjectManager.LinkProjectRequirements(projects, options, SampleProjects.ConfigurationDefault);

            var outputProjects = await _runnerService.InstallAsync(projects, options, CancellationToken.None);

            var integrationTestProject = Assert.Single(outputProjects);

            Assert.Equal(2, integrationTestProject.RequiredProjects.Count);

            var dotnetApp = Assert.Single(integrationTestProject.RequiredProjects, p => p.Framework == ProjectFramework.Dotnet);
            Assert.Equal(AppTask.Install, dotnetApp.Processes.First().Task);
            Assert.Equal(AppStatus.Success, dotnetApp.OutputStatus);

            var nodeApp = Assert.Single(integrationTestProject.RequiredProjects, p => p.Framework == ProjectFramework.Node);
            Assert.Equal(AppTask.Install, nodeApp.Processes.First().Task);
            Assert.Equal(AppStatus.Success, nodeApp.OutputStatus);

            _frameworkServiceProviderMock.Verify();
            _dotnetServiceMock.Verify();
            _nodeServiceMock.Verify();
        }

        [Fact]
        public async Task Install_will_not_install_a_dependency_if_it_is_also_a_top_level_filtered_project()
        {
            TestQueueService dependencyQueueService = new TestQueueService(_console);

            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(dependencyQueueService)
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var projects = SampleProjects.ConfigurationDefault.Projects;

            ProjectManager.LinkProjectRequirements(projects, TestOptions.Create(), SampleProjects.ConfigurationDefault);

            var outputProjects = await _runnerService.InstallAsync(projects, TestOptions.Create(), CancellationToken.None);

            var allAppsAndTests = SampleProjects.ConfigurationDefault.Projects;

            Assert.Equal(allAppsAndTests.Count, outputProjects.Count);

            foreach (var project in outputProjects)
            {
                if (project.RequiredProjects.Any())
                {
                    foreach (var requiredProject in project.RequiredProjects)
                    {
                        Assert.Empty(requiredProject.Processes);
                    }
                }
            }

            _frameworkServiceProviderMock.Verify();
            _dotnetServiceMock.Verify();
            _nodeServiceMock.Verify();
        }

        [Fact]
        public async Task Install_will_not_install_a_dependency_if_it_has_already_been_installed_as_another_project_dependency()
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
            options.Roles = test1.Roles;

            var config = new RegiConfig
            {
                Projects = new List<Project> { app, test1, test2 },
            };

            ProjectManager.LinkProjectRequirements(config.Projects, options, config);

            _frameworkServiceProviderMock.Setup(m => m.CreateScopedQueueService())
                .Returns(() => new TestQueueService(_console))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Install, AppStatus.Success))
                .Verifiable();

            var outputProjects = await _runnerService.InstallAsync(config.Projects, options, CancellationToken.None);

            Assert.Equal(3, outputProjects.Count); // app, test1, test2

            var appProcess = outputProjects[0].Processes.ElementAt(0);
            var test1RequiredProcess = outputProjects[1].RequiredProjects.ElementAt(0).Processes.ElementAt(0);
            var test2RequiredProcess = outputProjects[2].RequiredProjects.ElementAt(0).Processes.ElementAt(0);
            Assert.Same(appProcess, test1RequiredProcess);
            Assert.Same(appProcess, test2RequiredProcess);

            _frameworkServiceProviderMock.Verify();
            _dotnetServiceMock.Verify(m => m.Install(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact]
        public async Task Build_will_build_every_project_for_given_framework()
        {
            var backend = SampleProjects.Backend;
            var frontend = SampleProjects.Frontend;
            var projects = new[] { backend, frontend };

            var options = TestOptions.Create();

            var backendDirectoryPaths = backend.GetAppDirectoryPaths(_fileSystem);
            var backendBuildProccess = new AppProcess(new Process(), AppTask.Build, AppStatus.Success);
            _dotnetServiceMock.Setup(m => m.Build(backend, backendDirectoryPaths[0], options, It.IsAny<CancellationToken>()))
                .ReturnsAsync(backendBuildProccess)
                .Verifiable();

            var frontendDirectoryPaths = frontend.GetAppDirectoryPaths(_fileSystem);
            var frontendBuildProcess = new AppProcess(new Process(), AppTask.Build, AppStatus.Success);
            _nodeServiceMock.Setup(m => m.Build(frontend, frontendDirectoryPaths[0], options, It.IsAny<CancellationToken>()))
                .ReturnsAsync(frontendBuildProcess)
                .Verifiable();

            var output = await _runnerService.BuildAsync(projects, options, CancellationToken.None);

            Assert.Same(backend, output[0]);
            Assert.Same(backendBuildProccess, output[0].Processes.First());
            
            Assert.Same(frontend, output[1]);
            Assert.Same(frontendBuildProcess, output[1].Processes.First());

            _dotnetServiceMock.Verify();
            _nodeServiceMock.Verify();
        }

        [Fact]
        public async Task Build_will_build_async_by_default()
        {
            var projects = new[] { SampleProjects.Backend, SampleProjects.Backend };

            var options = TestOptions.Create();

            _dotnetServiceMock.Setup(m => m.Build(It.IsAny<Project>(), It.IsAny<string>(), options, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Build, AppStatus.Success))
                .Verifiable();

            await _runnerService.BuildAsync(projects, options, CancellationToken.None);

            Assert.Equal(projects.Length, _queueService.AsyncActions.Count);
            Assert.Empty(_queueService.SerialActions);

            _dotnetServiceMock.Verify();
        }

        [Fact]
        public async Task Build_will_build_in_serial_if_specified()
        {
            var projects = new[] { SampleProjects.Backend, SampleProjects.Backend };

            var options = TestOptions.Create();
            
            options.NoParallel = true;

            _dotnetServiceMock.Setup(m => m.Build(It.IsAny<Project>(), It.IsAny<string>(), options, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(new Process(), AppTask.Build, AppStatus.Success))
                .Verifiable();

            await _runnerService.BuildAsync(projects, options, CancellationToken.None);

            Assert.Equal(projects.Length, _queueService.SerialActions.Count);
            Assert.Empty(_queueService.AsyncActions);

            _dotnetServiceMock.Verify();
        }

        [Fact]
        public async Task Kill_calls_service_to_kill_for_each_ProjectFramework()
        {
            _nodeServiceMock.Setup(m => m.Kill(It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.Kill(It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();

            var projects = SampleProjects.ConfigurationGood.Projects.Concat(SampleProjects.ConfigurationGood.Projects).ToList();
            var options = TestOptions.Create();

            await _runnerService.KillAsync(projects, options, CancellationToken.None);

            _nodeServiceMock.Verify(m => m.Kill(options, CancellationToken.None), Times.Once);
            _dotnetServiceMock.Verify(m => m.Kill(options, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Kill_will_call_kill_for_every_type_of_ProjectFramework_if_there_are_no_projects()
        {
            _frameworkServiceProviderMock.Setup(m => m.GetAllProjectFrameworkTypes())
                .Returns(new List<ProjectFramework> { ProjectFramework.Dotnet, ProjectFramework.Node })
                .Verifiable();
            _dotnetServiceMock.Setup(m => m.Kill(It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();
            _nodeServiceMock.Setup(m => m.Kill(It.IsAny<CommandOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AppProcess(null, AppTask.Kill, AppStatus.Success))
                .Verifiable();

            var options = TestOptions.Create();

            await _runnerService.KillAsync(null, options, CancellationToken.None);

            _frameworkServiceProviderMock.Verify();
            _nodeServiceMock.Verify(m => m.Kill(options, CancellationToken.None), Times.Once);
            _dotnetServiceMock.Verify(m => m.Kill(options, CancellationToken.None), Times.Once);
        }
    }
}
