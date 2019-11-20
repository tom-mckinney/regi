using System;

namespace Regi
{
    public class RegiException : Exception
    {
        public RegiException()
        {
        }

        public RegiException(string message) : base(message)
        {
        }

        public RegiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
