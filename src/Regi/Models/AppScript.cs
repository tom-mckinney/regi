using System;
using System.Collections.Generic;
using System.Text;

namespace Regi.Models
{
    public class AppScript
    {
        public AppScript()
        {
        }

        public AppScript(string run, string path)
        {
            Run = run;
            Path = path;
        }

        public string Run { get; set; }

        public string Path { get; set; }
    }
}
