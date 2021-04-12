using Regi.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Runtime
{
    public class ProcessManager : IProcessManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly ConcurrentDictionary<Guid, IManagedProcess> _managedProcesses = new(); 

        public ProcessManager(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        internal IReadOnlyDictionary<Guid, IManagedProcess> ManagedProcesses => _managedProcesses;

        public ValueTask<IManagedProcess> CreateAsync(string fileName, string arguments, DirectoryInfo workingDirectory = null)
        {
            if (workingDirectory == null)
            {
                workingDirectory = new DirectoryInfo(_fileSystem.WorkingDirectory);
            }

            var managedProcess = new ManagedProcess(fileName, arguments, workingDirectory, null); // TODO: create ILogSink

            if (!_managedProcesses.TryAdd(managedProcess.Id, managedProcess))
            {
                throw new InvalidOperationException($"Managed process with Id of {managedProcess.Id} already exists"); // TODO: test this
            }

            return new ValueTask<IManagedProcess>(managedProcess);
        }

        public async ValueTask<bool> ShutdownAsync(Guid managedProcessId, CancellationToken cancellationToken)
        {
            if (!_managedProcesses.TryRemove(managedProcessId, out IManagedProcess managedProcess))
            {
                throw new InvalidOperationException($"No managed process with Id of {managedProcessId}");
            }

            await managedProcess.KillAsync(cancellationToken);

            return true;
        }
    }
}
