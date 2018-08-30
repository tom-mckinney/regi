using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Test.Utilities
{
    internal class TestCommandLineContext : CommandLineContext
    {
        public TestCommandLineContext()
        {
        }

        public TestCommandLineContext(IConsole console)
        {
            Console = console;
        }
    }
}
