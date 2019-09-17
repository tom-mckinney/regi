﻿using Moq;
using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Services
{
    public class SummaryServiceTests
    {
        private readonly TestConsole _console;

        public SummaryServiceTests(ITestOutputHelper outputHelper)
        {
            _console = new TestConsole(outputHelper);
        }

        ISummaryService CreateService()
        {
            return new SummaryService(new ProjectManager(_console, new Mock<ICleanupService>().Object), _console);
        }

        [Fact]
        public void PrintDomainSummary_prints_all_apps_and_tests()
        {
            var service = CreateService();

            var output = service.PrintDomainSummary(SampleProjects.ConfigurationGood, TestOptions.Create());

            Assert.Equal(SampleProjects.ConfigurationGood.Apps.Count, output.Apps.Count);
            Assert.Equal(SampleProjects.ConfigurationGood.Tests.Count, output.Tests.Count);

            Assert.Contains("Apps:", _console.LogOutput, StringComparison.InvariantCulture);
            Assert.Contains("Tests:", _console.LogOutput, StringComparison.InvariantCulture);
        }

        [Theory]
        [InlineData("node", 1, 0)]
        [InlineData("test", 0, 2)]
        [InlineData("SampleApp1", 1, 0)]
        public void PrintDomainSummary_prints_only_apps_or_tests_that_match_name_if_specified(string name, int appCount, int testCount)
        {
            var service = CreateService();

            var output = service.PrintDomainSummary(SampleProjects.ConfigurationGood, new RegiOptions { Name = name });

            Assert.Equal(appCount, output.Apps.Count);
            Assert.Equal(testCount, output.Tests.Count);

            if (appCount <= 0)
                Assert.DoesNotContain("Apps:", _console.LogOutput, StringComparison.InvariantCulture);
            if (testCount <= 0)
                Assert.DoesNotContain("Tests:", _console.LogOutput, StringComparison.InvariantCulture);
        }

        [Fact]
        public void Passing_test_with_single_path()
        {
            var projects = new List<Project>
            {
                SampleProjects.XunitTests
            };

            projects[0].Processes.Add(new AppProcess(null, AppTask.Test, AppStatus.Success) { Path = SampleProjects.XunitTests.Path });

            var service = CreateService();

            var outputSummary = service.PrintTestSummary(projects, TimeSpan.FromMilliseconds(100));

            Assert.Equal(1, outputSummary.SuccessCount);
            Assert.Equal(0, outputSummary.FailCount);
            Assert.Equal(0, outputSummary.UnknownCount);

            string expectedOutput = "";
            expectedOutput += $" PASS  {SampleProjects.XunitTests.Name}";
            expectedOutput += "Test projects: 1 succeeded, 1 totalElapsed time: 100ms";

            Assert.Equal(expectedOutput, _console.LogOutput.Replace(Environment.NewLine, string.Empty, StringComparison.InvariantCulture));
        }

        [Fact]
        public void Passing_test_collection_with_multiple_paths()
        {
            var projects = new List<Project>
            {
                SampleProjects.TestCollection
            };

            projects[0].Processes.Add(new AppProcess(null, AppTask.Test, AppStatus.Success) { Path = SampleProjects.TestCollection.Paths[0] });
            projects[0].Processes.Add(new AppProcess(null, AppTask.Test, AppStatus.Success) { Path = SampleProjects.TestCollection.Paths[1] });
            projects[0].Processes.Add(new AppProcess(null, AppTask.Test, AppStatus.Success) { Path = SampleProjects.TestCollection.Paths[2] });

            var service = CreateService();

            var outputSummary = service.PrintTestSummary(projects, TimeSpan.FromMilliseconds(100));

            Assert.Equal(1, outputSummary.SuccessCount);
            Assert.Equal(0, outputSummary.FailCount);
            Assert.Equal(0, outputSummary.UnknownCount);

            string expectedOutput = "";
            expectedOutput += $" PASS  {SampleProjects.TestCollection.Name}";
            expectedOutput += $"  PASS  {DirectoryUtility.GetDirectoryShortName(SampleProjects.TestCollection.Paths[2])}";
            expectedOutput += $"  PASS  {DirectoryUtility.GetDirectoryShortName(SampleProjects.TestCollection.Paths[1])}";
            expectedOutput += $"  PASS  {DirectoryUtility.GetDirectoryShortName(SampleProjects.TestCollection.Paths[0])}";
            expectedOutput += "Test projects: 1 succeeded, 1 totalElapsed time: 100ms";

            Assert.Equal(expectedOutput, _console.LogOutput.Replace(Environment.NewLine, string.Empty, StringComparison.InvariantCulture));
        }

        [Fact]
        public void Failing_test_collection_with_multiple_paths()
        {
            var projects = new List<Project>
            {
                SampleProjects.TestCollection
            };

            projects[0].Processes.Add(new AppProcess(null, AppTask.Test, AppStatus.Failure) { Path = SampleProjects.TestCollection.Paths[0] });
            projects[0].Processes.Add(new AppProcess(null, AppTask.Test, AppStatus.Success) { Path = SampleProjects.TestCollection.Paths[1] });
            projects[0].Processes.Add(new AppProcess(null, AppTask.Test, AppStatus.Success) { Path = SampleProjects.TestCollection.Paths[2] });

            var service = CreateService();

            var outputSummary = service.PrintTestSummary(projects, TimeSpan.FromMilliseconds(100));

            Assert.Equal(0, outputSummary.SuccessCount);
            Assert.Equal(1, outputSummary.FailCount);
            Assert.Equal(0, outputSummary.UnknownCount);

            string expectedOutput = "";
            expectedOutput += $" FAIL  {SampleProjects.TestCollection.Name}";
            expectedOutput += $"  PASS  {DirectoryUtility.GetDirectoryShortName(SampleProjects.TestCollection.Paths[2])}";
            expectedOutput += $"  PASS  {DirectoryUtility.GetDirectoryShortName(SampleProjects.TestCollection.Paths[1])}";
            expectedOutput += $"  FAIL  {DirectoryUtility.GetDirectoryShortName(SampleProjects.TestCollection.Paths[0])}";
            expectedOutput += "Test projects: 1 failed, 1 totalElapsed time: 100ms";

            Assert.Equal(expectedOutput, _console.LogOutput.Replace(Environment.NewLine, string.Empty, StringComparison.InvariantCulture));
        }
    }
}
