using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Regiment.Test.Utilities
{
    public class TestConsole : IConsole
    {
        public TestConsole(ITestOutputHelper output)
        {
            Out = new XunitTextWriter(output);
            Error = new XunitTextWriter(output);
        }

        public TextWriter Out { get; set; }

        public TextWriter Error { get; set; }

        public TextReader In => throw new NotImplementedException();

        public bool IsInputRedirected => throw new NotImplementedException();

        public bool IsOutputRedirected => throw new NotImplementedException();

        public bool IsErrorRedirected => throw new NotImplementedException();

        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }

        public event ConsoleCancelEventHandler CancelKeyPress
        {
            add { }
            remove { }
        }

        public void ResetColor()
        {
        }
    }
}
