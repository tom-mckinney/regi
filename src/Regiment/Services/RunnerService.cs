using Regiment.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regiment.Services
{
    public interface IRunnerService
    {
        IList<DotnetProcess> RunAsync(DirectoryInfo directory);
        IList<DotnetProcess> RunAsync(FileInfo startupFile);
    }

    public class RunnerService : IRunnerService
    {
        public IList<DotnetProcess> RunAsync(DirectoryInfo directory)
        {
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"Could not find directory: {directory.FullName}");
            }


            throw new NotImplementedException();
        }

        public IList<DotnetProcess> RunAsync(FileInfo startupFile)
        {
            throw new NotImplementedException();
        }
    }
}
