using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Commands
{
    public abstract class CommandBase
    {
        [Argument(0, Description = "Name of the project")]
        public string Name { get; set; }

        [Option(Description = "Search pattern")]
        public string SearchPattern { get; set; }

        public abstract int OnExecute();
    }
}
