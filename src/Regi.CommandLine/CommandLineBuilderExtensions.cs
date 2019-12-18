using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Text;

namespace Regi.CommandLine
{
    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder AddCommand<TCommand>(this CommandLineBuilder builder)
            where TCommand : CommandBase, new()
        {
            return builder.AddCommand(new TCommand().InternalCommand);
        }
    }
}
