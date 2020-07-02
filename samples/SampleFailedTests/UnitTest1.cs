using System;
using Xunit;

namespace SampleFailedTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.False(true);
        }
    }
}
