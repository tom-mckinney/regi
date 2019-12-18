using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.CommandLine.Models
{
    public partial class OptionsBase
    {
        public string Name { get; set; }

        public DirectoryInfo ConfigurationPath { get; set; }

        public List<string> Exclude { get; set; }
    }
}
