using Regi.Abstractions;
using System;

namespace Regi.Test.Extensions
{
    public static class AppTaskExtensions
    {
        public static string GetCommand(this AppTask task)
        {
            switch (task)
            {
                case AppTask.Test:
                    return "test";
                case AppTask.Start:
                    return "start";
                case AppTask.Install:
                    return "install";
                default:
                    throw new ArgumentException("Task must be valid AppTask value", nameof(task));
            }
        }
    }
}
