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

        public DataReceivedEventHandler DefaultOutputDataRecieved(string name)
        {
            return new DataReceivedEventHandler((o, e) =>
            {
                _console.WriteLine(name + ": " + e.Data);
            });
        }

        public DataReceivedEventHandler DefaultErrorDataReceived(string name, AppProcess output)
        {
            return new DataReceivedEventHandler((o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    output.Status = AppStatus.Failure;
                    _console.WriteErrorLine(name + ": " + e.Data);
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
