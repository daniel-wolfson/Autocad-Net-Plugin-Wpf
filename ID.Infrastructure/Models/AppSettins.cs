using ID.Infrastructure;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Intellidesk.Data.General
{
    [DataContract]
    [KnownType(typeof(BindableBase))]
    public class AppSettings<T> : BindableBase where T : PluginSettings, new()
    {
        private static string workPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IntelliDesk");
        public static string WorkPath { get { return workPath; } }

        public static string FileName { get; set; } = "appsettings";

        public void Save()
        {
            Save(this as T);
        }

        public static void Save(T pSettings)
        {
            pSettings.SaveToJsonFile(Path.Combine(WorkPath, FileName));
        }

        public static T Build(string workPath = null, bool isDevelopmentEnvironment = true)
        {
            var EnvironmentName = isDevelopmentEnvironment ? "development" : "production";

            //IConfigurationBuilder builder = new ConfigurationBuilder()
            //   .SetBasePath(workPath ?? WorkPath)
            //   .AddJsonFile($"{FileName}.json", optional: true, reloadOnChange: true)
            //   .AddJsonFile($"{FileName}.{EnvironmentName}.json", optional: true, reloadOnChange: true);
            //.AddEnvironmentVariables();

            AppDomain.CurrentDomain.AssemblyResolve += AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;
            //IConfigurationRoot Configuration = builder.Build();
            AppDomain.CurrentDomain.AssemblyResolve -= AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;

            //var tokenConfig = Configuration.GetSection("AppUserToken");
            //var token = tokenConfig.Value;
            //if (string.IsNullOrEmpty(token))
            //{
            //    token = Util.CreateToken(AppUser.Create(new AppUserDetails() { UserName = CommandNames.UserGroup + "User" }));
            //}
            //if (!Util.VerifyToken(token))
            //{
            //    token = Util.CreateToken(AppUser.Default);
            //}

            //T objSettings = Configuration.Get<T>();
            //objSettings.AppUserToken = token;
            //objSettings.Name = char.ToUpper(CommandNames.UserGroup[0]) + CommandNames.UserGroup.Substring(1).ToLower();
            //return objSettings;
            return null;
        }

        public static T Load(string jsonFilePath = null, string jsonFileName = null)
        {
            T t = new T();

            jsonFileName = Path.Combine(jsonFilePath ?? WorkPath, jsonFileName ?? FileName);

            if (File.Exists(jsonFileName) && new FileInfo(jsonFileName).Length > 0)
            {
                t = jsonFileName.CreateFromJsonFile<T>();
                t.ScaleFactors = t.ScaleFactors.Distinct().ToList();
            }
            else
                Save(t);

            t.ShowAllFolders = false;

            if (t.WorkItems != null && t.WorkItems.Any())
            {
                var workItems = new List<IWorkItem>(t.WorkItems);
                foreach (var workItem in t.WorkItems)
                {
                    if (!File.Exists(workItem.FullPath))
                        workItems.Remove(workItem);
                }
                t.WorkItems = workItems;
            }

            if (t.IncludeFolders == null || !t.IncludeFolders.Any())
                t.IncludeFolders = new List<string>() { "C:\\" };

            if (!Directory.Exists(t.CurrentFolder) || string.IsNullOrEmpty(t.CurrentFolder))
                t.CurrentFolder = t.IncludeFolders[0];

            return t;
        }

        public static T Get()
        {
            return null;//Plugin.GetService<T>();
        }



    }
}
