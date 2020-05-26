using McMaster.Extensions.CommandLineUtils;
using Regi.Extensions;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Frameworks
{
    public interface IDjango : IFramework
    {
    }

    public class Django : FrameworkBase, IDjango
    {
        public Django(IConsole console, IPlatformService platformService) : base(console, platformService, "python")
        {
        }

        public override ProjectFramework Framework => ProjectFramework.Django;

        public override IEnumerable<string> ProcessNames => new[] { "python" };

        protected override IEnumerable<string> FrameworkWarningIndicators => new[]
        {
            "Watching for file changes with StatReloader"
        };

        protected override CommandDictionary FrameworkOptions => new CommandDictionary
        {
            {
                FrameworkCommands.Install, new[]
                {
                    "-r ./requirements.txt"
                }
            }
        };

        protected override void ApplyFrameworkOptions(StringBuilder builder, string command, Project project, CommandOptions options)
        {
            if (project.Port.HasValue && project.Roles.Contains(ProjectRole.App))
            {
                if (command == FrameworkCommands.Django.RunServer)
                {
                    builder.AppendCliOption($"0.0.0.0:{project.Port}");
                }
            }

            base.ApplyFrameworkOptions(builder, command, project, options);
        }

        public override string StartCommand => "./manage.py runserver";

        public override string TestCommand => "./manage.py test";

        public override async Task<AppProcess> Install(Project project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken)
        {
            AppProcess install = CreateProcess(InstallCommand, project, appDirectoryPath, options, "pip");

            install.Start();

            await install.WaitForExitAsync(cancellationToken);

            return install;
        }
    }
}
