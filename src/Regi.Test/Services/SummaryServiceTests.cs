using Regi.Models;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
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
            return new SummaryService(_console);
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

            string expectedOutput = "\r";
            expectedOutput += $" PASS  {SampleProjects.XunitTests.Name}\r";
            expectedOutput += "\rTest projects: 1 succeeded, 1 total\rElapsed time: 100ms\r\r";

            Assert.Equal(expectedOutput, _console.LogOutput);
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

            string expectedOutput = "\r";
            expectedOutput += $" PASS  {SampleProjects.TestCollection.Name}\r";
            expectedOutput += $"   PASS  {SampleProjects.TestCollection.Paths[2]}\r";
            expectedOutput += $"   PASS  {SampleProjects.TestCollection.Paths[1]}\r";
            expectedOutput += $"   PASS  {SampleProjects.TestCollection.Paths[0]}\r";
            expectedOutput += "\rTest projects: 1 succeeded, 1 total\rElapsed time: 100ms\r\r";

            Assert.Equal(expectedOutput, _console.LogOutput);
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

            string expectedOutput = "\r";
            expectedOutput += $" FAIL  {SampleProjects.TestCollection.Name}\r";
            expectedOutput += $"   PASS  {SampleProjects.TestCollection.Paths[2]}\r";
            expectedOutput += $"   PASS  {SampleProjects.TestCollection.Paths[1]}\r";
            expectedOutput += $"   FAIL  {SampleProjects.TestCollection.Paths[0]}\r";
            expectedOutput += "\rTest projects: 1 failed, 1 total\rElapsed time: 100ms\r\r";

            Assert.Equal(expectedOutput, _console.LogOutput);
        }
    }
}
