﻿using Regi.Models;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Regi.Test
{
    public partial class TestBase
    {
        protected static void CleanupApp(AppProcess app)
        {
            if (app == null)
            {
                throw new ArgumentException("App cannot be null", nameof(app));
            }

            int processId = app.ProcessId;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessUtility.KillProcessWindows(processId, out string _, out string _);
            }
            else
            {
                var children = new HashSet<int>();
                ProcessUtility.GetAllChildIdsUnix(processId, children);
                foreach (var childId in children)
                {
                    ProcessUtility.KillProcessUnix(childId, out string _, out string _);
                }

                ProcessUtility.KillProcessUnix(processId, out string _, out string _);
            }
        }
    }
}