﻿using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Regi.Commands
{
    [Command("install")]
    public class InstallCommand : CommandBase
    {
        private readonly IRunnerService _runnerService;

        public InstallCommand(IRunnerService runnerService, ICleanupService cleanupService, IConsole console)
            : base(cleanupService, console)
        {
            _runnerService = runnerService;
        }

        public override int OnExecute()
        {
            Projects = _runnerService.Install(Options);

            return Projects
                .Where(p => p.Process?.Status == AppStatus.Failure)
                .Count();
        }
    }
}
