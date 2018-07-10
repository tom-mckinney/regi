using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Regiment.Test.Utilities
{
    public class XunitTextWriter : TextWriter
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly StringBuilder _sb = new StringBuilder();

        public XunitTextWriter(ITestOutputHelper testOutput, bool preserveOutput = false)
        {
            PreserveOutput = preserveOutput;
            _testOutput = testOutput;
        }

        public bool PreserveOutput { get; }

        public string Output { get; set; }

        public override Encoding Encoding => Encoding.Unicode;

        public override void Write(char ch)
        {
            if (ch == '\n')
            {
                if (PreserveOutput)
                {
                    Output += (_sb.ToString() + NewLine);
                }

                _testOutput.WriteLine(_sb.ToString());
                _sb.Clear();
            }
            else
            {
                _sb.Append(ch);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sb.Length > 0)
                {
                    _testOutput.WriteLine(_sb.ToString());
                    _sb.Clear();
                }
            }

            base.Dispose(disposing);
        }
    }
}
