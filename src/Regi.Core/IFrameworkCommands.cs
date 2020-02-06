using System.Collections.Generic;

namespace Regi
{
    public interface IFrameworkCommands
    {
        string InstallCommand { get; }
        string StartCommand { get; }
        string TestCommand { get; }
        string BuildCommand { get; }
        string KillCommand { get; }
        string PublishCommand { get; }
        string PackageCommand { get; }
        IEnumerable<string> ProcessNames { get; }
    }
}
