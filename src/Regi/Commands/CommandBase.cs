using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Regi.Commands
{
    public abstract class CommandBase : RegiOptions
    {
        protected readonly IConsole _console;

        public CommandBase(IConsole console)
        {
            _console = console;
        }

        public IList<Project> Projects { get; set; }

        public RegiOptions Options
        {
            get => this;
        }

        public abstract int OnExecute();
    }
}
