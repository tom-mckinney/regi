using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Services
{
    public interface IRuntimeInfo
    {
        bool IsWindows { get; }
        bool IsMac { get; }
        bool IsLinux { get; }
        bool IsWindowsLinux { get; }

        string NewLine { get; }
    }

    public class RuntimeInfo : IRuntimeInfo
    {
        public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public bool IsWindowsLinux => IsLinux && RuntimeInformation.OSDescription.Contains("Microsoft");

        public string NewLine => Environment.NewLine;
    }
}
