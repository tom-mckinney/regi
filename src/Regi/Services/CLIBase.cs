using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regi.Services
{
    public abstract class CLIBase
    {
        private IConsole _console;

        public CLIBase(IConsole console)
        {
            _console = console;
        }

        public DataReceivedEventHandler DefaultOutputDataRecieved()
        {
            return new DataReceivedEventHandler((o, e) =>
            {
                _console.WriteLine(e.Data);
            });
        }

        public DataReceivedEventHandler DefaultErrorDataReceived(AppProcess output)
        {
            return new DataReceivedEventHandler((o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    output.Status = AppStatus.Failure;
                    _console.WriteErrorLine(e.Data);
                }
            });
        }

        public EventHandler DefaultExited(AppProcess output)
        {
            return new EventHandler((o, e) =>
            {
                output.End = DateTimeOffset.UtcNow;

                if (output.Status == AppStatus.Running)
                {
                    output.Status = AppStatus.Success;
                }
            });
        }
    }
}
