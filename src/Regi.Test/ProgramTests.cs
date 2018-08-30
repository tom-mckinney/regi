using Regi.Test.Utilities;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test
{
    public class ProgramTests
    {
        private readonly TestConsole _console;

        public ProgramTests(ITestOutputHelper testOutput)
        {
            _console = new TestConsole(testOutput);
        }

        [Theory]
        [InlineData]
        [InlineData("test")]
        public void All_commands_have_accurate_help_response(params string[] args)
        {
            args = args.Concat(new string[] { "--help" }).ToArray();

            int response = Program.MainWithConsole(_console, args);

            Assert.Equal(0, response);
        }
    }
}
