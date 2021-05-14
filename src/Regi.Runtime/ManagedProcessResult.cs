using Regi.Abstractions;

namespace Regi.Runtime
{
    public record ManagedProcessResult : IManagedProcessResult
    {
        public ManagedProcessResult()
        {
        }

        public ManagedProcessResult(int exitCode, ILogSink logSink)
        {
            ExitCode = exitCode;

            if (logSink.TryGetStandardOutput(out string standardOutput))
            {
                StandardOutput = standardOutput;
            }

            if (logSink.TryGetStandardError(out string standardError))
            {
                StandardError = standardError;
            }
        }

        public int ExitCode { get; init; }

        public string StandardOutput { get; init; }

        public string StandardError { get; init; }
    }
}
