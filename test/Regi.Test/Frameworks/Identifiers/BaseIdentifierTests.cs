using Moq;
using Regi.Abstractions;
using Regi.Frameworks.Identifiers;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Regi.Test.Identifiers
{
    public class BaseIdentifierTests
    {
        protected Mock<IFileSystem> FileSystemMock { get; } = new Mock<IFileSystem>(MockBehavior.Strict);

        protected virtual IIdentifier CreateTestClass()
        {
            return new TestBaseIdentifier(FileSystemMock.Object);
        }

        public static IEnumerable<object[]> DotnetProjects => new List<Project>
        {
            SampleProjects.ClassLib,
            SampleProjects.Backend,
            SampleProjects.XunitTests,
        }.Select(p => new object[] { p.Name, p });

        public static IEnumerable<object[]> NodeProjects => new List<Project>
        {
            SampleProjects.Frontend,
            SampleProjects.JestTests,
        }.Select(p => new object[] { p.Name, p });

        protected virtual void ShouldHaveIdentified(IProject expectedProject, bool wasIdentified)
        {
            Assert.True(wasIdentified);
        }

        protected virtual void ShouldHaveMatched(IProject expectedProject, bool wasMatch)
        {
            Assert.True(wasMatch);
        }

        [Theory]
        [MemberData(nameof(DotnetProjects))]
        [MemberData(nameof(NodeProjects))]
        public async Task<IProject> Identify_base_project(string name, IProject expectedProject)
        {
            if (expectedProject == null)
                throw new ArgumentNullException(nameof(expectedProject));

            var expectedProjectContents = GetFileSystemDictionary(expectedProject);

            string expectedPath = $"./{name}";
            FileSystemMock.Setup(m => m.GetRelativePath(expectedProjectContents.Path))
                .Returns(expectedPath);

            var identifier = CreateTestClass();

            bool shouldIdentify = await identifier.ShouldIdentify(null, expectedProjectContents);
            ShouldHaveIdentified(expectedProject, shouldIdentify);

            bool isMatch = await identifier.IsMatchAsync(null, expectedProjectContents);
            ShouldHaveMatched(expectedProject, isMatch);

            var actualProject = await identifier.CreateOrModifyAsync(null, expectedProjectContents); // no project created yet

            FileSystemMock.VerifyAll();

            Assert.Equal(name, actualProject.Name); // used for labeling test
            Assert.Equal(expectedProjectContents.Name, actualProject.Name);
            Assert.Equal(expectedPath, actualProject.Paths.Single());

            return actualProject;
        }

        protected static FileSystemDictionary GetFileSystemDictionary(IProject project)
        {
            project = project ?? throw new ArgumentNullException(nameof(project));

            string path;

            if (!string.IsNullOrEmpty(project.Path))
            {
                path = project.Path;
            }
            else
            {
                path = project.Paths.Single();
            }

            DirectoryInfo projectDirectory = File.Exists(path) ? new FileInfo(path).Directory : new DirectoryInfo(path);
            return new FileSystemDictionary(projectDirectory);
        }

        private class TestBaseIdentifier : BaseIdentifier
        {
            public TestBaseIdentifier(IFileSystem fileSystem) : base(fileSystem)
            {
            }

            public override ValueTask<bool> IsMatchAsync(IProject project, IFileSystemDictionary directoryContents)
            {
                return new ValueTask<bool>(true);
            }

            public override ValueTask<bool> ShouldIdentify(IProject project, IFileSystemDictionary directoryContents)
            {
                return new ValueTask<bool>(true);
            }
        }
    }
}
