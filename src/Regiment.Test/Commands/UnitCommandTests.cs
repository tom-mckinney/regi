using Regiment.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regiment.Test.Commands
{
    public class UnitCommandTests
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly TestConsole _console;

        public UnitCommandTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _console = new TestConsole(testOutput);
        }

        [Fact]
        public void Will_run_dotnet_test_on_all_projects()
        {
            string[] args = new string[] { "unit" };

            int response = Program.MainWithConsole(_console, args);

            _testOutput.WriteLine("Completed unit tests");

            Assert.Equal(0, response);
        }
    }
}
