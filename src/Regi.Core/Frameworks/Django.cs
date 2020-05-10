using McMaster.Extensions.CommandLineUtils;
using Regi.Models;
using Regi.Services;
using System;
using System.Collections.Generic;
using System.Text;

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

        public override string StartCommand => "./manage.py runserver";
    }
}
