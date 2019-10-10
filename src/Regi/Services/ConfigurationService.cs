using Newtonsoft.Json;
using Regi.Extensions;
using Regi.Models;
using Regi.Models.Exceptions;
using Regi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Regi.Services
{
    public interface IConfigurationService
    {
        StartupConfig GetConfiguration(RegiOptions options);
    }

    public class ConfigurationService : IConfigurationService
    {
        public StartupConfig GetConfiguration(RegiOptions options)
        {
            DirectoryInfo directory;

            if (!string.IsNullOrWhiteSpace(options?.ConfigurationPath))
            {
                string configDirectory = DirectoryUtility.GetDirectoryPath(options.ConfigurationPath, true, "directory");
                DirectoryUtility.SetWorkingDirectory(configDirectory);
                directory = new DirectoryInfo(configDirectory);
            }
            else
            {
                directory = new DirectoryInfo(DirectoryUtility.WorkingDirectory);
            }

            if (directory?.Exists == false)
            {
                throw new DirectoryNotFoundException($"Could not find directory: {directory.FullName}");
            }

            FileInfo startupFile = directory.GetOneOfFiles("regi.json", "startup.json");

            if (startupFile == null || !startupFile.Exists)
            {
                throw new RegiException($"Could not find regi.json or startup.json in directory: {directory.FullName}");
            }

            using (StreamReader sr = new StreamReader(startupFile.OpenRead()))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = JsonSerializer.CreateDefault();
                serializer.MissingMemberHandling = MissingMemberHandling.Error;

                try
                {
                    var startupConfig = serializer.Deserialize<StartupConfig>(reader);

                    startupConfig.Path = startupFile.FullName;

                    return startupConfig;
                }
                catch (Exception e)
                {
                    throw new RegiException($"Configuration file was not properly formatted: {startupFile.FullName}{Environment.NewLine}{e.Message}", e);
                }
            }
        }
    }
}
