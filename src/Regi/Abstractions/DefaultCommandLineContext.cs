using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Abstractions
{
    public class DefaultCommandLineContext : CommandLineContext
    {
        public DefaultCommandLineContext() { }

        public DefaultCommandLineContext(IConsole console)
        {
            Console = console;
        }
    }
}
