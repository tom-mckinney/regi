using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Test.Helpers
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
