using Moq;
using Regi.Abstractions;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test
{
    public class ProjectManagerTests : TestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ICleanupService> _cleanupServiceMock = new Mock<ICleanupService>();
        private readonly Mock<IProjectFilter> _projectFilterMock = new Mock<IProjectFilter>(MockBehavior.Strict);

        public ProjectManagerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public IProjectManager CreateProjectManager()
        {
            return new ProjectManager(new TestConsole(_output), _cleanupServiceMock.Object, _projectFilterMock.Object);
        }

        [Fact]
        public void FilterByOptions_includes_all_non_optional_projects_by_default()
        {
            var projects = new[]
            {
                SampleProjects.Backend,
                SampleProjects.Frontend,
                SampleProjects.XunitTests,
                SampleProjects.IntegrationTests
            };

            _projectFilterMock.Setup(m => m.FilterByOptional(projects))
                .Returns(Array.Empty<Project>());

            var projectManager = CreateProjectManager();

            var options = TestOptions.Create();

            var filteredProjects = projectManager.FilterByOptions(projects, options);

            Assert.Empty(filteredProjects); // empty array is being returned by mock

            _projectFilterMock.VerifyAll();
        }

        [Fact]
        public void FilterByOptions_does_not_exclude_optional_projects_if_include_optional_is_specified()
        {
            var optionalProject = new Project
            {
                Optional = true
            };

            var options = TestOptions.Create();

            options.IncludeOptional = true;

            var projectManager = CreateProjectManager();

            var filteredProjects = projectManager.FilterByOptions(new List<Project> { optionalProject }, options);

            Assert.NotEmpty(filteredProjects);
            _projectFilterMock.Verify(m => m.FilterByOptional(It.IsAny<IEnumerable<Project>>()), Times.Never);
            _projectFilterMock.VerifyAll();
        }

        [Fact]
        public void FilterByOptions_filters_by_name_if_specified()
        {
            var projects = new[]
            {
                SampleProjects.Backend
            };

            var options = TestOptions.Create();

            options.Name = "foo";

            var expectedFilteredProjects = Array.Empty<Project>();

            _projectFilterMock.Setup(m => m.FilterByName(projects, "foo"))
                .Returns(expectedFilteredProjects);

            _projectFilterMock.Setup(m => m.FilterByOptional(expectedFilteredProjects))
                .Returns(expectedFilteredProjects);

            var projectManager = CreateProjectManager();

            var filteredProjects = projectManager.FilterByOptions(projects, options);

            Assert.Empty(filteredProjects);

            _projectFilterMock.VerifyAll();
        }

        [Fact]
        public void FilterByOptions_filters_by_labels_if_specified()
        {
            var projects = new[]
            {
                SampleProjects.Backend
            };

            var labels = new List<string> { "silly-label" };

            var options = TestOptions.Create();

            options.Labels = labels;

            var expectedFilteredProjects = Array.Empty<Project>();

            _projectFilterMock.Setup(m => m.FilterByLabels(projects, labels))
                .Returns(expectedFilteredProjects);

            _projectFilterMock.Setup(m => m.FilterByOptional(expectedFilteredProjects))
                .Returns(expectedFilteredProjects);

            var projectManager = CreateProjectManager();

            var filteredProjects = projectManager.FilterByOptions(projects, options);

            Assert.Empty(filteredProjects);

            _projectFilterMock.VerifyAll();
        }

        [Fact]
        public void FilterByOptions_filters_by_exclusions_if_specified()
        {
            var projects = new[]
            {
                SampleProjects.Backend
            };

            var exclusions = new List<string> { "Backend" };

            var options = TestOptions.Create();

            options.Exclude = exclusions;

            var expectedFilteredProjects = Array.Empty<Project>();

            _projectFilterMock.Setup(m => m.FilterByExclusions(projects, exclusions))
                .Returns(expectedFilteredProjects);

            _projectFilterMock.Setup(m => m.FilterByOptional(expectedFilteredProjects))
                .Returns(expectedFilteredProjects);

            var projectManager = CreateProjectManager();

            var filteredProjects = projectManager.FilterByOptions(projects, options);

            Assert.Empty(filteredProjects);

            _projectFilterMock.VerifyAll();
        }

        [Theory]
        [InlineData(ProjectRole.App)]
        [InlineData(ProjectRole.Test)]
        public void FilterByOption_filters_by_roles_if_specified(params ProjectRole[] roles)
        {
            var projects = new[]
            {
                SampleProjects.Backend,
                SampleProjects.XunitTests
            };

            var expectedFilteredProjects = Array.Empty<Project>();

            _projectFilterMock.Setup(m => m.FilterByRoles(projects, roles))
                .Returns(expectedFilteredProjects);

            _projectFilterMock.Setup(m => m.FilterByOptional(expectedFilteredProjects))
                .Returns(expectedFilteredProjects);

            var projectManager = CreateProjectManager();

            var options = TestOptions.Create();

            options.Roles = roles;

            var filteredProjects = projectManager.FilterByOptions(projects, options);

            Assert.Empty(filteredProjects);
            _projectFilterMock.VerifyAll();
        }
    }
}
