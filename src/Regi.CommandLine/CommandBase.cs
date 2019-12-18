using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Regi.CommandLine
{
    public abstract class CommandBase
    {
        public abstract Command InternalCommand { get; }

        protected abstract string Name { get; }

        protected virtual string Description { get; } = null;

        protected virtual string[] Aliases { get; } = new string[] { };

        protected virtual List<Argument> Arguments { get; } = Directives.DefaultArguments;

        protected virtual List<Option> Options { get; } = Directives.DefaultOptions;
    }

    public abstract class CommandBase<TOptions> : CommandBase
        where TOptions : class
    {
        public CommandBase()
        {
        }

        public override Command InternalCommand
        {
            get
            {
                var command = new Command(Name, Description)
                {
                    Handler = CommandHandler.Create<TOptions, IHost>((options, host) =>
                    {
                        return RunAsync(options, host.Services);
                    })
                };

                foreach (var alias in Aliases)
                {
                    command.AddAlias(alias);
                }

                foreach (var argument in Arguments)
                {
                    command.AddArgument(argument);
                }

                foreach (var option in Options)
                {
                    command.AddOption(option);
                }

                return command;
            }
        }

        public abstract Task RunAsync(TOptions options, IServiceProvider services);
    }
}
