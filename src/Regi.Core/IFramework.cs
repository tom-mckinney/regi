using Regi.Abstractions;
using Regi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Regi
{
    public interface IFramework : IFrameworkCommands
    {
        ProjectFramework Framework { get; }

        Task<IAppProcess> Install(IProject project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken);
        Task<IAppProcess> Start(IProject project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken);
        Task<IAppProcess> Test(IProject project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken);
        Task<IAppProcess> Build(IProject project, string appDirectoryPath, CommandOptions options, CancellationToken cancellationToken);
        Task<IAppProcess> Kill(CommandOptions options, CancellationToken cancellationToken);
    }
}
