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
        StartupConfig GetConfiguration();
    }

    public class ConfigurationService : IConfigurationService
    {
        public StartupConfig GetConfiguration()
        {
            DirectoryInfo directory = new DirectoryInfo(DirectoryUtility.TargetDirectoryPath);

            if (!directory.Exists)
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
                    return serializer.Deserialize<StartupConfig>(reader);
                }
                catch (Exception e)
                {
                    throw new RegiException($"Configuration file was not properly formatted: {startupFile.FullName}{Environment.NewLine}{e.Message}", e);
                }
            }
        }
    }
}
