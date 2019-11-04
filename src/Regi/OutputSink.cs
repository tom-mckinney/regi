using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regi
{
    public class OutputSink
    {
        public OutputSink(AppProcess appProcess)
        {
            if (appProcess?.Process == null)
            {
                throw new ArgumentException("AppProcess cannot be null and must have a running Process", nameof(appProcess));
            }

            appProcess.Process.OutputDataReceived += HandleOutputDataReceived;
            appProcess.Process.ErrorDataReceived += HandleErrorDataReceived;
        }

        private void HandleErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void HandleOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
