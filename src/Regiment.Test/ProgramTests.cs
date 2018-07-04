using Regiment.Test.Utilities;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Regiment.Test
{
    public class ProgramTests
    {
        private readonly TestConsole _console;

        public ProgramTests(ITestOutputHelper output)
        {
            _console = new TestConsole(output);
        }

        [Theory]
        [InlineData]
        [InlineData("unit")]
        public void All_commands_have_accurate_help_response(params string[] args)
        {
            args = args.Concat(new string[] { "--help" }).ToArray();

            int response = Program.MainWithConsole(_console, args);

            Assert.Equal(0, response);
        }
    }
}
