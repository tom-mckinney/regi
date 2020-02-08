using Regi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi
{
    public static class FrameworkCommands
    {
        public const string Any = "*";
        public const string Install = "install";
        public const string Start = "start";
        public const string Test = "test";
        public const string Build = "build";
        public const string Kill = "kill";
        public const string Publish = "publish";
        public const string Package = "package";

        public static class DotnetCore
        {
            public const string Run = "run";
            public const string Test = FrameworkCommands.Test;
            public const string Build = FrameworkCommands.Build;
            public const string Restore = "restore";
            public const string Publish = "publish";
            public const string ShutdownBuildServer = "build-server shutdown";
        }

        public static class Node
        {
            public const string Start = FrameworkCommands.Start;
            public const string Test = FrameworkCommands.Test;
            public const string Build = "run build";
            public const string Install = FrameworkCommands.Install;
        }

        public static class RubyOnRails
        {
            public const string Server = "server";
        }

        public static AppTask GetAppTask(string command)
        {
            switch (command)
            {
                case Start:
                case DotnetCore.Run:
                    return AppTask.Start;
                case Test:
                    return AppTask.Test;
                case Build:
                case Node.Build:
                    return AppTask.Build;
                case DotnetCore.Restore:
                case Node.Install:
                    return AppTask.Install;
                default:
                    throw new ArgumentException("Command must be a valid FrameworkCommand", nameof(command));
            }
        }
    }
}
