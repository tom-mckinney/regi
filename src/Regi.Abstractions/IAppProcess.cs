using Regi.Abstractions.Options;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface IAppProcess
    {
        AppTask Task { get; set; }

        AppStatus Status { get; set; }

        Process Process { get; }

        int ProcessId { get; }

        string ProcessName { get; }

        int? Port { get; set; }

        string Path { get; set; }

        DateTimeOffset? StartTime { get; }

        DateTimeOffset? EndTime { get; set; }

        bool KillOnExit { get; set; }

        bool Verbose { get; set; }

        bool RawOutput { get; }

        bool ErrorDataHandled { get; }

        bool OutputDataHandled { get; }

        Action<int> OnKill { get; set; }

        Task WaitForExitAsync(CancellationToken cancellationToken);

        void Start();

        void Kill(TimeSpan timeout, OptionsBase options = null);
    }
}
