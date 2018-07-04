using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Regiment.Commands
{
    [Command("unit")]
    public class UnitCommand
    {
        [Argument(0, Description = "Name of the project directory or file")]
        public string Name { get; set; }

        public int OnExecute()
        {
            return 0;
        }
    }
}
