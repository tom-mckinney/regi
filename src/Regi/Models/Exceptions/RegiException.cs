using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Models.Exceptions
{
    public class RegiException : Exception
    {
        public RegiException()
        {
        }

        public RegiException(string message) : base(message)
        {
        }
    }
}
