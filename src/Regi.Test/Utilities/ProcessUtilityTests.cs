using Regi.Test.Helpers;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Regi.Test.Utilities
{
    [Collection(TestCollections.NoParallel)]
    public class ProcessUtilityTests
    {
        [Fact]
        public void CreateProcess_returns_process_with_fileName_and_arguments_in_target_directory()
        {
            string targetDirectory = PathHelper.SampleDirectoryPath("ConfigurationGood");

            DirectoryUtility.SetWorkingDirectory(targetDirectory);

            var process = ProcessUtility.CreateProcess("wumbo", "--foo bar");

            Assert.Equal("wumbo", process.StartInfo.FileName);
            Assert.Equal("--foo bar", process.StartInfo.Arguments);
            Assert.Equal(targetDirectory, process.StartInfo.WorkingDirectory);
            Assert.True(process.StartInfo.RedirectStandardOutput);
            Assert.True(process.StartInfo.RedirectStandardError);
            Assert.True(process.StartInfo.RedirectStandardInput);
            Assert.False(process.StartInfo.UseShellExecute);
        }

        [Fact]
        public void CreateProcess_returns_process_with_fileName_and_arguments_in_working_directory_is_relative_path()
        {
            string targetDirectory = PathHelper.SampleDirectoryPath("ConfigurationBad");
            string workingDirectory = PathHelper.SampleDirectoryPath("ConfigurationGood");

            DirectoryUtility.SetWorkingDirectory(targetDirectory);

            var process = ProcessUtility.CreateProcess("wumbo", "--foo bar", "../ConfigurationGood"); // relative

            Assert.Equal("wumbo", process.StartInfo.FileName);
            Assert.Equal("--foo bar", process.StartInfo.Arguments);
            Assert.Equal(workingDirectory, process.StartInfo.WorkingDirectory);
            Assert.True(process.StartInfo.RedirectStandardOutput);
            Assert.True(process.StartInfo.RedirectStandardError);
            Assert.True(process.StartInfo.RedirectStandardInput);
            Assert.False(process.StartInfo.UseShellExecute);
        }

        [Fact]
        public void CreateProcess_returns_process_with_fileName_and_arguments_in_working_directory_is_absolute_path()
        {
            string targetDirectory = PathHelper.SampleDirectoryPath("ConfigurationBad");
            string workingDirectory = PathHelper.SampleDirectoryPath("ConfigurationGood");

            DirectoryUtility.SetWorkingDirectory(targetDirectory);

            var process = ProcessUtility.CreateProcess("wumbo", "--foo bar", workingDirectory); // absolute

            Assert.Equal("wumbo", process.StartInfo.FileName);
            Assert.Equal("--foo bar", process.StartInfo.Arguments);
            Assert.Equal(workingDirectory, process.StartInfo.WorkingDirectory);
            Assert.True(process.StartInfo.RedirectStandardOutput);
            Assert.True(process.StartInfo.RedirectStandardError);
            Assert.True(process.StartInfo.RedirectStandardInput);
            Assert.False(process.StartInfo.UseShellExecute);
        }
    }
}
