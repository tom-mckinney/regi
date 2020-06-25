using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.CommandLine
{
    public abstract class CommandBase : CommandOptions
    {
        protected readonly IProjectManager _projectManager;
        protected readonly IConfigurationService _configurationService;
        protected readonly IConsole _console;

        public CommandBase(IProjectManager projectManager, IConfigurationService configurationService, IConsole console)
        {
            _projectManager = projectManager;
            _configurationService = configurationService;
            _console = console;
        }

        public CommandOptions Options
        {
            get => this;
        }

        /// <summary>
        /// Setting to false will allow this command to be executed in a directory without a Regi configuration file
        /// </summary>
        public virtual bool RequireRegiConfig => true;

        /// <summary>
        /// Setting to false will prevent this command from filtering projects and tracking them in the <see cref="ProjectManager"/>.
        /// </summary>
        public virtual bool FilterProjects => true;

        public RegiConfig Config { get; protected set; }

        protected abstract Func<RegiConfig, IEnumerable<Project>> GetTargetProjects { get; }

        protected virtual async Task BeforeExecuteAsync()
        {
            try
            {
                if (RequireRegiConfig)
                {
                    Config = await _configurationService.GetConfigurationAsync(Options);

                    Options.VariableList = new EnvironmentVariableDictionary(Config);

                    if (FilterProjects)
                    {
                        _projectManager.FilterAndTrackProjects(Options, Config, GetTargetProjects);
                    }
                }
            }
            catch (Exception e)
            {
                if (RequireRegiConfig)
                {
                    throw;
                }
                else if (Options.Verbose)
                {
                    _console.WriteWarningLine(e.Message);
                }
            }
        }

        protected abstract Task<int> ExecuteAsync(IList<Project> projects, CancellationToken cancellationToken);

        protected virtual void AfterExecute()
        {
            //_projectManager.KillAllProcesses(Options);
        }

        public virtual async Task<int> OnExecute()
        {
            await BeforeExecuteAsync();

            int statusCode = await ExecuteAsync(_projectManager.Projects, _projectManager.CancellationTokenSource.Token);

            AfterExecute();

            return statusCode;
        }
    }
}
