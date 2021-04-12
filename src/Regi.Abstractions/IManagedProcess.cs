using System;
using System.IO;
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
}
