using McMaster.Extensions.CommandLineUtils;
using Regi.Abstractions;
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

        public IServiceMesh ServiceMesh { get; protected set; }

        protected abstract Func<IServiceMesh, IEnumerable<IProject>> GetTargetProjects { get; }

        protected virtual async Task BeforeExecuteAsync()
        {
            try
            {
                if (RequireRegiConfig)
                {
                    ServiceMesh = await _configurationService.GetConfigurationAsync(Options);

                    Options.VariableList = new EnvironmentVariableDictionary(ServiceMesh);

                    if (FilterProjects)
                    {
                        _projectManager.FilterAndTrackProjects(Options, ServiceMesh, GetTargetProjects);
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

        protected abstract Task<int> ExecuteAsync(IList<IProject> projects, CancellationToken cancellationToken);

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
