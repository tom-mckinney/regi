using Regi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Regi
{
    public interface IFramework : IFrameworkCommands
    {
        ProjectFramework Framework { get; }

        Task<AppProcess> Install(Project project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken);
        Task<AppProcess> Start(Project project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken);
        Task<AppProcess> Test(Project project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken);
        Task<AppProcess> Build(Project project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken);
        Task<AppProcess> Kill(CommandOptions options, CancellationToken cancellationToken);
    }
}
