using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Services
{
    public interface IRuntimeInfo
    {
        bool IsWindows { get; }
    }

    public class RuntimeInfo : IRuntimeInfo
    {
        public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
