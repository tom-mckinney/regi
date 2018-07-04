using System;
using Xunit;

namespace Regiment.Test
{
    public class ProgramTests
    {
        [Theory]
        [InlineData("unit")]
        [InlineData("--help")]
        public void Main_returns_successful_response(params string[] args)
        {
            int response = Program.Main(args);

            Assert.Equal(0, response);
        }
    }
}
