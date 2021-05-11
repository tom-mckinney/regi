using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface IProcessManager
    {
        ValueTask<IManagedProcess> CreateAsync(string serviceName, string fileName, string arguments, DirectoryInfo workingDirectory = null);
        ValueTask<bool> ShutdownAsync(Guid managedProcessId, CancellationToken cancellationToken);
    }
}
