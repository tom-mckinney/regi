using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Test.Helpers
{
    public static class PathHelper
    {
        private static readonly char Slash = Path.DirectorySeparatorChar;

        static internal string SampleDirectoryPath(string name)
        {
            string path = $"{Directory.GetCurrentDirectory()}{Slash}_SampleProjects_{Slash}{name}";

            return path;
        }
    }
}
