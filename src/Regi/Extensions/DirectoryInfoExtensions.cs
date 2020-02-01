using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Regi.Extensions
{
    public static class DirectoryInfoExtensions
    {
        public static FileInfo GetOneOfFiles(this DirectoryInfo directory, params string[] filePatterns)
        {
            if (directory == null)
            {
                throw new ArgumentException("Directory cannot be null", nameof(directory));
            }

            if (filePatterns == null || filePatterns.Length <= 0)
            {
                throw new ArgumentException("Must specify at least one file pattern", nameof(filePatterns));
            }

            foreach (var pattern in filePatterns)
            {
                var file = directory.GetFiles(pattern).FirstOrDefault();
                if (file != null)
                {
                    return file;
                }
            }

            return null;
        }
    }
}
