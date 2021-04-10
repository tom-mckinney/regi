using System;

namespace SampleAppError
{
    class Program
    {
        static void Main(string[] args)
        {
            throw new Exception("What is dead may never die.");
        }
    }
}
