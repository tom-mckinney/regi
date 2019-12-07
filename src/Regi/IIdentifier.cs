using Regi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Regi
{
    public interface IIdentifier
    {
        ValueTask<bool> ShouldIdentify(Project project, IFileSystemDictionary directoryContents);

        ValueTask<bool> IsMatchAsync(Project project, IFileSystemDictionary directoryContents);

        ValueTask<Project> CreateOrModifyAsync(Project project, IFileSystemDictionary directoryContents);
    }
}
