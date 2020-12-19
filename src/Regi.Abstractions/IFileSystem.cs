using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Regi.Abstractions
{
    public interface IFileSystem
    {
        string WorkingDirectory { get; set; }

        string GetDirectoryPath(string path, bool throwIfNotFound = true, string targetObj = "project");
        string GetRelativePath(string path);
        ValueTask<FileInfo> CreateConfigFileAsync(IServiceMesh config);
        List<FileInfo> FindAllProjectFiles();
        string FindFileOrDirectory(string fileName);
        IEnumerable<DirectoryInfo> GetChildDirectories(DirectoryInfo directory);
        IFileSystemDictionary GetAllChildren(DirectoryInfo directory);
    }
}
