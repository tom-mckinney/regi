using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Constants
{
    public static class FrameworkCommands
    {
        public static class Dotnet
        {
            public const string Start = "run";
            public const string Test = "test";
            public const string Install = "restore";
        }

        public static class Node
        {
            public const string Start = "start";
            public const string Test = "test";
            public const string Install = "install";
        }
    }
}
