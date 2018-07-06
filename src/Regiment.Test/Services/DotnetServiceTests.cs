using Regiment.Services;
using Regiment.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regiment.Test.Services
{
    public class DotnetServiceTests
    {
        private readonly IDotnetService _service;

        public DotnetServiceTests(ITestOutputHelper output)
        {
            _service = new DotnetService(new TestConsole(output));
        }

        [Fact]
        public void TestProject_prints_all_output_when_verbose()
        {

        }
    }
}
