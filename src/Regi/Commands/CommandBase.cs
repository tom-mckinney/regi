using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;

namespace Regi.Commands
{
    public abstract class CommandBase : RegiOptions
    {
        protected readonly IConsole _console;
        protected readonly ICleanupService _cleanupService;

        public CommandBase(ICleanupService cleanupService, IConsole console)
        {
            _console = console;
            _cleanupService = cleanupService;

            if (console != null)
            {
                console.CancelKeyPress += (o, e) =>
                {
                    ShutdownProccesses();
                };
            }
        }

        public IList<Project> Projects { get; set; }

        public void ShutdownProccesses()
        {
            _console.WriteEmphasizedLine("Shutting down processes");

            if (Projects != null)
            {
                foreach (var project in Projects)
                {
                    if (project.Process != null)
                    {
                        project.Process.Dispose();
                    }
                }
            }

            _cleanupService.ShutdownBuildServers(Options);
        }

        public RegiOptions Options
        {
            get => this;
        }

        public abstract int OnExecute();
    }
}
