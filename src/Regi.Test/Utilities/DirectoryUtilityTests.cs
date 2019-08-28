using Regi.Test.Helpers;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Regi.Test.Utilities
{
    public class DirectoryUtilityTests
    {
        [Fact]
        public void GetDirectoryPath_returns_full_path_for_directory()
        {
            string expectedDirectoryPath = PathHelper.SampleDirectoryPath("SampleSuccessfulTests");

            Assert.Equal(expectedDirectoryPath, DirectoryUtility.GetDirectoryPath(expectedDirectoryPath));


        }

        [Fact]
        public void GetDirectoryPath_returns_full_path_for_file()
        {
            string expectedDirectoryPath = PathHelper.SampleDirectoryPath("SampleSuccessfulTests");
            string filePath = PathHelper.SampleDirectoryPath("SampleSuccessfulTests/SampleSuccessfulTests.csproj");

            Assert.Equal(expectedDirectoryPath, DirectoryUtility.GetDirectoryPath(filePath));
        }

        [Fact]
        public void GetDirectoryPath_throws_if_directory_does_not_exist()
        {
            string path = PathHelper.SampleDirectoryPath("FAKE_DIRECTORY");

            Assert.Throws<DirectoryNotFoundException>(() => DirectoryUtility.GetDirectoryPath(path));
        }

        [Fact]
        public void GetDirectoryPath_throws_if_file_does_not_exist()
        {
            string path = PathHelper.SampleDirectoryPath("FAKE_DIRECTORY/Fake.csproj");

            Assert.Throws<DirectoryNotFoundException>(() => DirectoryUtility.GetDirectoryPath(path));
        }

        [Fact]
        public void GetDirectoryPath_with_file_that_does_not_exist_does_not_throw_if_throwIfNotFound_is_false()
        {
            string path = PathHelper.SampleDirectoryPath("FAKE_DIRECTORY/Fake.csproj");

            Assert.Null(DirectoryUtility.GetDirectoryPath(path, false));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetDirectoryPath_throws_if_path_is_null(string path)
        {
            Assert.Throws<ArgumentException>(() => DirectoryUtility.GetDirectoryPath(path));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetDirectoryPath_does_not_throw_if_path_is_null_and_throwIfNotFound_is_false(string path)
        {
            Assert.Null(DirectoryUtility.GetDirectoryPath(path, false));
        }
    }
}
