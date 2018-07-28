﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Regiment.Test.Utilities
{
    public class XunitTextWriter : TextWriter
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly Action<string> _callback;
        private readonly StringBuilder _sb = new StringBuilder();

        public XunitTextWriter(ITestOutputHelper testOutput, Action<string> callback = null)
        {
            _testOutput = testOutput;
            _callback = callback;
        }

        public string Output { get; set; }

        public override Encoding Encoding => Encoding.Unicode;

        public override void Write(char ch)
        {
            if (ch == '\n')
            {
                _callback?.Invoke(_sb.ToString());

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
