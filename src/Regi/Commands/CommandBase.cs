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
        private readonly IProjectManager projectManager;
        private readonly IConfigurationService configurationService;
        protected readonly IConsole console;

        public CommandBase(IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
        {
            this.projectManager = projectManager;
            this.configurationService = configurationService;
            this.console = console;
        }

        public RegiOptions Options
        {
            get => this;
        }

        /// <summary>
        /// Setting to false will allow this command to be executed in a directory without a Regi configuration file
        /// </summary>
        public virtual bool RequireStartupConfig => true;

        /// <summary>
        /// Setting to false will prevent this command from filtering projects and tracking them in the <see cref="ProjectManager"/>.
        /// </summary>
        public virtual bool FilterProjects => true;

        public StartupConfig Config { get; protected set; }

        protected abstract Func<StartupConfig, IEnumerable<Project>> GetTargetProjects { get; }

        protected virtual void BeforeExecute()
        {
            try
            {
                Config = configurationService.GetConfiguration();

                Options.VariableList = new VariableList(Config);

                if (FilterProjects)
                {
                    projectManager.FilterAndTrackProjects(Options, Config, GetTargetProjects);
                }
            }
            catch (Exception e)
            {
                if (RequireStartupConfig)
                {
                    throw;
                }
                else if (Options.Verbose)
                {
                    console.WriteWarningLine(e.Message);
                }
            }
        }

        protected abstract int Execute(IList<Project> projects);

        protected virtual void AfterExecute()
        {
            projectManager.KillAllProcesses(Options);
        }

        public virtual int OnExecute()
        {
            BeforeExecute();

            int statusCode = Execute(projectManager.Projects);

            AfterExecute();

            return statusCode;
        }
    }
}
