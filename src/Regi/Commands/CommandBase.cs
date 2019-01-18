using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using System;
using System.Collections.Generic;

namespace Regi.Commands
{
    public abstract class CommandBase : CommandOptions
    {
        public IList<Project> Projects { get; set; }

        public CommandBase(IConsole console)
        {
            if (console != null)
            {
                console.CancelKeyPress += (o, e) =>
                {
                    ShutdownProccesses();
                };
            }
        }

        public void ShutdownProccesses()
        {
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
        }

        public CommandOptions Options
        {
            get => new CommandOptions
            {
                Name = this.Name,
                SearchPattern = this.SearchPattern,
                Verbose = this.Verbose
            };
        }

        public abstract int OnExecute();
    }
}
