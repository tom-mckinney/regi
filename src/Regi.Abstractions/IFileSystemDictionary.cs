using System.Collections.Generic;
using System.IO;

namespace Regi.Abstractions
{
    public interface IFileSystemDictionary : IDictionary<string, FileSystemInfo>
    {
        string Name { get; }
        string Path { get; }
    }
}
