using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System.Collections.Generic;

namespace Regi.Frameworks
{
    public interface IRubyOnRails : IFramework
    {
    }

    public class RubyOnRails : FrameworkBase, IRubyOnRails
    {
        public RubyOnRails(IConsole console, IPlatformService platformService) : base(console, platformService, "rails")
        {
        }

        public override ProjectFramework Framework => ProjectFramework.RubyOnRails;
        public override IEnumerable<string> ProcessNames => new[] { "ruby" };

        public override string StartCommand => FrameworkCommands.RubyOnRails.Server;
    }
}
