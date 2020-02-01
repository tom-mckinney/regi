using Regi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi
{
    public static class FrameworkCommands
    {
        public const string Any = "*";
        public const string Test = "test";

        public static class Dotnet
        {
            public const string Run = "run";
            public const string Test = FrameworkCommands.Test;
            public const string Build = "build";
            public const string Restore = "restore";
            public const string Publish = "publish";
            public const string ShutdownBuildServer = "build-server shutdown";
        }

        public static class Node
        {
            public const string Start = "start";
            public const string Test = FrameworkCommands.Test;
            public const string Build = "run build";
            public const string Install = "install";
        }

        public static AppTask GetAppTask(string command)
        {
            switch (command)
            {
                case Dotnet.Run:
                case Node.Start:
                    return AppTask.Start;
                case Test:
                    return AppTask.Test;
                case Dotnet.Build:
                case Node.Build:
                    return AppTask.Build;
                case Dotnet.Restore:
                case Node.Install:
                    return AppTask.Install;
                default:
                    throw new ArgumentException("Command must be a valid FrameworkCommand", nameof(command));
            }
        }
    }
}
