using Regi.Runtime;
using System;
using System.Runtime.InteropServices;

namespace Regi.Test.Stubs
{
    public class StubbedRuntimeInfo : IRuntimeInfo
    {
        public bool IsWindows { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public bool IsMac { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public bool IsLinux { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public bool IsWindowsLinux { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSDescription.Contains("Microsoft", StringComparison.InvariantCultureIgnoreCase);

        public string NewLine { get; set; } = Environment.NewLine;
    }
}
