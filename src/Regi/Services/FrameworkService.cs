using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Regi.Services
{
    public interface IFrameworkService
    {
        AppProcess InstallProject(Project project, CommandOptions options);
        AppProcess StartProject(Project project, CommandOptions options);
        AppProcess TestProject(Project project, CommandOptions options);
    }

    public abstract class FrameworkService : IFrameworkService
    {
        private IConsole _console;

        public FrameworkService(IConsole console)
        {
            _console = console;
        }

        public abstract AppProcess InstallProject(Project project, CommandOptions options);
        public abstract AppProcess StartProject(Project project, CommandOptions options);
        public abstract AppProcess TestProject(Project project, CommandOptions options);

        public virtual DataReceivedEventHandler DefaultOutputDataRecieved(string name)
        {
            return new DataReceivedEventHandler((o, e) =>
            {
                _console.WriteLine(name + ": " + e.Data);
            });
        }

        public virtual DataReceivedEventHandler DefaultErrorDataReceived(string name, AppProcess output)
        {
            return new DataReceivedEventHandler((o, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    output.Status = AppStatus.Failure;
                    _console.WriteErrorLine(name + ": " + e.Data);
                }
            });
        }

        public virtual EventHandler DefaultExited(AppProcess output)
        {
            return new EventHandler((o, e) =>
            {
                output.EndTime = DateTimeOffset.UtcNow;

                if (output.Status == AppStatus.Running)
                {
                    output.Status = AppStatus.Success;
                }
            });
        }

        public virtual string BuildCommand(string command, Project project, CommandOptions options)
        {
            if (project?.Commands != null && project.Commands.TryGetValue(command, out string customCommand))
            {
                command = customCommand;
            }

            StringBuilder builder = new StringBuilder();

            builder.Append(command);

            if (project?.Options?.Count > 0)
            {
                foreach (var commandOption in project.Options)
                {
                    if (commandOption.Key == "*" || commandOption.Key == command)
                    {

                        builder.Append(' ').AppendJoin(' ', commandOption.Value);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(options?.Arguments))
            {
                builder.Append(' ').Append(FormatAdditionalArguments(options.Arguments));
            }

            return builder.ToString();
        }

        protected virtual string FormatAdditionalArguments(string args)
        {
            return args;
        }

        protected abstract IDictionary<string, IList<string>> FrameworkDefaultOptions { get; }

        protected virtual void ApplyFrameworkDefaults(Project project)
        {

        }
    }
}
