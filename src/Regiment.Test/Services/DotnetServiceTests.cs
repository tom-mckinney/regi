using Regiment.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Regiment.Test.Services
{
    public class DotnetServiceTests
    {
        private readonly IDotnetService _service;

        public DotnetServiceTests()
        {
            _service = new DotnetService();
        }

        [Fact]
        public void Test_runs_test_and_prints_errors()
        {

        }
    }
}
