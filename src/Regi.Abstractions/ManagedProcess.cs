using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface IManagedProcess
    {
        Guid Id { get; }
        string FileName { get; }
        string Arguments { get; }
        DirectoryInfo WorkingDirectory { get; }

        Task StartAsync(CancellationToken cancellationToken);
        Task WaitForExitAsync(CancellationToken cancellationToken);
        Task KillAsync(CancellationToken cancellationToken);
    }

    public class ManagedProcess : IManagedProcess
    {
        public ManagedProcess(string fileName, string arguments, DirectoryInfo workingDirectory)
        {
            Id = Guid.NewGuid();
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
        }

        public Guid Id { get; protected set; }

        public string FileName { get; protected set; }

        public string Arguments { get; protected set; }

        public DirectoryInfo WorkingDirectory { get; protected set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task WaitForExitAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task KillAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
