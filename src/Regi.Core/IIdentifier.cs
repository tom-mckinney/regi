using Regi.Abstractions;
using System.Threading.Tasks;

namespace Regi
{
    public interface IIdentifier
    {
        ValueTask<bool> ShouldIdentify(IProject project, IFileSystemDictionary directoryContents);

        ValueTask<bool> IsMatchAsync(IProject project, IFileSystemDictionary directoryContents);

        ValueTask<IProject> CreateOrModifyAsync(IProject project, IFileSystemDictionary directoryContents);
    }
}
