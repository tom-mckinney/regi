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
        private TestConsole _console;

        public UnitCommandTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
        }

        [Fact]
        public void Will_run_dotnet_test_on_all_projects()
        {
            string[] args = new string[] { "unit" };

            int response = Program.MainWithConsole(_console, args);

            Assert.Equal(0, response);
        }
    }
}
