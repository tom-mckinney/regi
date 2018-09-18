using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xunit;
using Regi.Utilities;

namespace Regi.Test.Utilities
{
    public class NpmExeTests
    {
        [Fact]
        public void FindsTheNodePath()
        {
            var npmPath = NpmExe.FullPath;
            Assert.NotNull(npmPath);
            Assert.True(File.Exists(npmPath), "The file did not exist");
            Assert.True(Path.IsPathRooted(npmPath), "The path should be rooted");
            Assert.Equal("npm", Path.GetFileNameWithoutExtension(npmPath), ignoreCase: true);
        }
    }
}
