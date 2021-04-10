using Regi.Abstractions;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Regi.Test
{
    public partial class TestBase
    {
        protected static readonly TestFileSystem _fileSystem = new TestFileSystem();

        protected static void CleanupApp(IAppProcess app)
        {
            if (app == null)
            {
                throw new ArgumentException("App cannot be null", nameof(app));
            }

            int processId = app.ProcessId;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessUtility.KillProcessWindows(processId, _fileSystem, out string _, out string _);
            }
            else
            {
                var children = new HashSet<int>();
                ProcessUtility.GetAllChildIdsUnix(processId, children, _fileSystem);
                foreach (var childId in children)
                {
                    ProcessUtility.KillProcessUnix(childId, _fileSystem, out string _, out string _);
                }

                ProcessUtility.KillProcessUnix(processId,  _fileSystem, out string _, out string _);
            }
        }
    }
}
