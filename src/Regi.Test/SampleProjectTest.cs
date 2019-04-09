using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Test
{
    public abstract class SampleProjectTest
    {
        protected readonly char Slash = Path.DirectorySeparatorChar;

        protected virtual string SampleDirectoryPath(string name)
        {
            string path = $"{Directory.GetCurrentDirectory()}{Slash}_SampleProjects_{Slash}{name}";

            return path;
        }
    }
}
