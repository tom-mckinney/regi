using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Regi.Services;
using Regi.Test.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Test
{
    public class TestFileSystem : FileSystem, IFileSystem
    {
        public TestFileSystem() : this(null)
        {
        }

        public TestFileSystem(IConsole console) : base(console)
        {
            WorkingDirectory = PathHelper.ProjectRootPath;
        }
    }
}
