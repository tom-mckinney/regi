using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Regi.Commands
{
    [Command("version")]
    public class VersionCommand : CommandBase
    {
        private readonly IConsole _console;

        public VersionCommand(IConsole console)
            : base(console)
        {
            _console = console;
        }

        public override int OnExecute()
        {
            var version = typeof(Program).Assembly.GetName().Version;

            _console.WriteEmphasizedLine($"Regi version: {version.Major}.{version.Minor}.{version.Build}");

            return 0;
        }
    }
}
