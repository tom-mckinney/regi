using McMaster.Extensions.CommandLineUtils;
using Regi.Services;
using Regi.Test.Helpers;

namespace Regi.Test
{
    public class TestFileSystem : FileSystem
    {
        public TestFileSystem() : this(null)
        {
        }

        public TestFileSystem(IConsole console) : base(console)
        {
            WorkingDirectory = PathHelper.RegiTestRootPath;
        }
    }
}
