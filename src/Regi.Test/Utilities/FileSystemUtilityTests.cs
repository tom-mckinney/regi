using Regi.Test.Helpers;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace Regi.Test.Utilities
{
    public class FileSystemUtilityTests
    {
        [Theory]
        [MemberData(nameof(SinglePaths))]
        public void GetFileOrDirectory_returns_single_file_or_directory(string path, Type type)
        {
            var fileOrDirectory = FileSystemUtility.GetFileOrDirectory(path);

            Assert.NotNull(fileOrDirectory);
            Assert.IsType(type, fileOrDirectory);
            Assert.True(fileOrDirectory.Exists);
        }

        [Theory]
        [InlineData("wumbo", typeof(DirectoryNotFoundException))]
        [InlineData("wumbo.csproj", typeof(FileNotFoundException))]
        public void GetFileOrDirectory_throws_file_or_directory_not_found_exception_depending_on_path(string path, Type exceptionType)
        {
            Assert.Throws(exceptionType, () => FileSystemUtility.GetFileOrDirectory(path));
        }

        [Fact]
        public void GetFilesOrDirectories_returns_list_of_files_or_directories_for_each_path()
        {
            var paths = SinglePaths.Select(o => (string)o[0]).ToList();

            var allFilesAndDirectories = FileSystemUtility.GetFilesOrDirectories(paths);

            Assert.Equal(SinglePaths.Count, allFilesAndDirectories.Count);
            for (int i = 0; i < allFilesAndDirectories.Count; i++)
            {
                var fileOrDirectory = allFilesAndDirectories[i];
                Type fileOrDirectoryType = (Type)SinglePaths[i][1];

                Assert.IsType(fileOrDirectoryType, fileOrDirectory);
                Assert.True(fileOrDirectory.Exists);
            }
        }

        public static IList<object[]> SinglePaths => new List<object[]>
        {
            new object[] { PathHelper.SampleDirectoryPath("ConfigurationGood"), typeof(DirectoryInfo) },
            new object[] { PathHelper.SampleDirectoryPath($"SampleApp{PathHelper.Slash}SampleApp.csproj"), typeof(FileInfo) },
            new object[] { PathHelper.SampleDirectoryPath($"SampleNodeApp{PathHelper.Slash}package.json"), typeof(FileInfo) },
        };
    }
}
