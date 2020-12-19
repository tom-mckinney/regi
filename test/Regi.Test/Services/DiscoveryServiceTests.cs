using Moq;
using Regi.Abstractions;
using Regi.Models;
using Regi.Services;
using Regi.Test.Extensions;
using Regi.Test.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Regi.Test.Services
{
    public class DiscoveryServiceTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
        private readonly IReadOnlyList<Mock<IIdentifier>> _identifierMocks = new List<Mock<IIdentifier>>
        {
            new Mock<IIdentifier>(MockBehavior.Strict),
            new Mock<IIdentifier>(MockBehavior.Strict),
            new Mock<IIdentifier>(MockBehavior.Strict)
        }.AsReadOnly();

        private IDiscoveryService CreateTestClass()
        {
            return new DiscoveryService(
                _fileSystemMock.Object,
                _identifierMocks.Select(m => m.Object)
            );
        }

        [Fact]
        public async Task IdentifyProject_calls_every_Identifier_on_directory()
        {
            var project = SampleProjects.Backend;
            var directory = new DirectoryInfo(PathHelper.GetSampleProjectPath("Backend"));
            var fileSystemObjects = new FileSystemDictionary(directory);

            _fileSystemMock.Setup(m => m.GetAllChildren(directory))
                .Returns(fileSystemObjects);

            for (int i = 0; i < _identifierMocks.Count; i++)
            {
                Project expectedProject = (i == 0 ? null : project); // project is created by first identifer
                bool isMatch = i == 0 || i == 1; // only the first and second match

                _identifierMocks[i].Setup(m => m.ShouldIdentify(expectedProject, fileSystemObjects))
                    .ReturnsAsync(true);

                _identifierMocks[i].Setup(m => m.IsMatchAsync(expectedProject, fileSystemObjects))
                    .ReturnsAsync(isMatch);
            }

            _identifierMocks[0].Setup(m => m.CreateOrModifyAsync(null, fileSystemObjects))
                .ReturnsAsync(project);

            _identifierMocks[1].Setup(m => m.CreateOrModifyAsync(project, fileSystemObjects))
                .ReturnsAsync(project);

            var service = CreateTestClass();

            var actualProject = await service.IdentifyProjectAsync(directory);

            Assert.Same(project, actualProject);
            _fileSystemMock.VerifyAll();
            _identifierMocks.VerifyAll();
        }

        [Fact]
        public async Task IdentiyProject_only_identifies_if_it_should()
        {
            var directory = new DirectoryInfo(PathHelper.RegiTestRootPath);
            var fileSystemObjects = new FileSystemDictionary(directory);

            _fileSystemMock.Setup(m => m.GetAllChildren(directory))
                .Returns(fileSystemObjects);

            foreach (var identifer in _identifierMocks)
            {
                identifer.Setup(m => m.ShouldIdentify(It.IsAny<Project>(), fileSystemObjects))
                    .ReturnsAsync(false);
            }

            var service = CreateTestClass();

            var actualProject = await service.IdentifyProjectAsync(directory);
            
            Assert.Null(actualProject);
            _fileSystemMock.VerifyAll();
            _identifierMocks.VerifyAll();
        }

        [Fact]
        public async Task IdentifyProject_only_creates_if_match()
        {
            var directory = new DirectoryInfo(PathHelper.RegiTestRootPath);
            var fileSystemObjects = new FileSystemDictionary(directory);

            _fileSystemMock.Setup(m => m.GetAllChildren(directory))
                .Returns(fileSystemObjects);

            foreach (var identifer in _identifierMocks)
            {
                identifer.Setup(m => m.ShouldIdentify(It.IsAny<Project>(), fileSystemObjects))
                    .ReturnsAsync(true);

                identifer.Setup(m => m.IsMatchAsync(It.IsAny<Project>(), fileSystemObjects))
                    .ReturnsAsync(false);
            }

            var service = CreateTestClass();

            var actualProject = await service.IdentifyProjectAsync(directory);

            Assert.Null(actualProject);
            _fileSystemMock.VerifyAll();
            _identifierMocks.VerifyAll();
        }

        [Fact]
        public async Task IdentifyAllProjects_identifies_starting_directory()
        {
            var project = SampleProjects.Frontend;
            var directory = new DirectoryInfo(project.Path);
            var fileSystemObjects = new FileSystemDictionary(directory);

            _fileSystemMock.Setup(m => m.GetAllChildren(directory))
                .Returns(fileSystemObjects);

            for (int i = 0; i < _identifierMocks.Count; i++)
            {
                var identifier = _identifierMocks[i];
                var expectedProject = i == 0 ? null : project;

                identifier.Setup(m => m.ShouldIdentify(expectedProject, fileSystemObjects))
                    .ReturnsAsync(true);
                identifier.Setup(m => m.IsMatchAsync(expectedProject, fileSystemObjects))
                    .ReturnsAsync(true);
                identifier.Setup(m => m.CreateOrModifyAsync(expectedProject, fileSystemObjects))
                    .ReturnsAsync(project);
            }

            var service = CreateTestClass();

            var allProjects = await service.IdentifyAllProjectsAsync(directory);

            var actualProject = Assert.Single(allProjects);
            Assert.Same(project, actualProject);

            _fileSystemMock.VerifyAll();
            _identifierMocks.VerifyAll();
        }

        [Fact]
        public async Task IdentifyAllProjects_identifies_child_directories_if_starting_directory_is_not_project()
        {
            var repositoryRoot = new DirectoryInfo(PathHelper.SampleProjectsRootPath);
            var repositoryRootContents = new FileSystemDictionary(repositoryRoot);

            var project = SampleProjects.Frontend;
            var frontendDirectory = new DirectoryInfo(project.Path);
            var frontendDirectoryContents = new FileSystemDictionary(frontendDirectory);

            _fileSystemMock.Setup(m => m.GetAllChildren(repositoryRoot)) // crawls through repo root first
                .Returns(repositoryRootContents);
            _fileSystemMock.Setup(m => m.GetChildDirectories(repositoryRoot))
                .Returns(new[] { frontendDirectory });
            _fileSystemMock.Setup(m => m.GetAllChildren(frontendDirectory))
                .Returns(frontendDirectoryContents);

            for (int i = 0; i < _identifierMocks.Count; i++)
            {
                var identifier = _identifierMocks[i];
                var expectedProject = i == 0 ? null : project;

                identifier.Setup(m => m.ShouldIdentify(null, repositoryRootContents))
                    .ReturnsAsync(true); // no reason not to check
                identifier.Setup(m => m.IsMatchAsync(null, repositoryRootContents))
                    .ReturnsAsync(false); // not a project directory

                identifier.Setup(m => m.ShouldIdentify(expectedProject, frontendDirectoryContents))
                    .ReturnsAsync(true);
                identifier.Setup(m => m.IsMatchAsync(expectedProject, frontendDirectoryContents))
                    .ReturnsAsync(true);
                identifier.Setup(m => m.CreateOrModifyAsync(expectedProject, frontendDirectoryContents))
                    .ReturnsAsync(project);
            }

            var service = CreateTestClass();

            var allProjects = await service.IdentifyAllProjectsAsync(repositoryRoot); // executed in repo root without a project

            var actualProject = Assert.Single(allProjects);
            Assert.Same(project, actualProject);

            _fileSystemMock.VerifyAll();
            _identifierMocks.VerifyAll();
        }

        [Fact]
        public async Task IdentifyAllProjects_ignores_files_in_DiscoverIgnore()
        {
            var repositoryRoot = new DirectoryInfo(PathHelper.SampleProjectsRootPath);
            var repositoryRootContents = new FileSystemDictionary(repositoryRoot);

            _fileSystemMock.Setup(m => m.GetAllChildren(repositoryRoot)) // crawls through repo root first
                .Returns(repositoryRootContents);
            _fileSystemMock.Setup(m => m.GetChildDirectories(repositoryRoot))
                .Returns(Constants.DiscoverIgnore.Select(v => new DirectoryInfo(v)));

            foreach (var identifier in _identifierMocks)
            {
                identifier.Setup(m => m.ShouldIdentify(null, repositoryRootContents))
                    .ReturnsAsync(true);
                identifier.Setup(m => m.IsMatchAsync(null, repositoryRootContents))
                    .ReturnsAsync(false);
            }

            var service = CreateTestClass();

            var allProjects = await service.IdentifyAllProjectsAsync(repositoryRoot);

            Assert.Empty(allProjects);
            _fileSystemMock.VerifyAll();
            _identifierMocks.VerifyAll();
        }
    }
}
