using Regi.Extensions;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Regi.Test.Extensions
{
    public class IConsoleExtensionTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;

        public IConsoleExtensionTests(ITestOutputHelper output)
        {
            _output = output;
            _console = new TestConsole(_output);
        }

        [Theory]
        [InlineData(ConsoleLineStyle.Normal, 1)]
        [InlineData(ConsoleLineStyle.LineBefore, 2)]
        [InlineData(ConsoleLineStyle.LineAfter, 2)]
        [InlineData(ConsoleLineStyle.LineBeforeAndAfter, 3)]
        public void Console_extensions_apply_line_style(ConsoleLineStyle style, int newLineCount)
        {
            _console.WriteEmphasizedLine("Testo", style);

            Assert.Equal(newLineCount, _console.LogOutput.Count(o => o == '\r'));
        }
    }
}
