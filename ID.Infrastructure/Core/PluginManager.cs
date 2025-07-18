using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Intellidesk.Infrastructure.General
{
    public interface IPluginManager1
    {
        void Initialize(Assembly assembly, string projectName);
        bool Compose(object part, Assembly assembly);
        string Name { get; set; }
        string NameMsg { get; set; }
        bool IsComposed { get; set; }
        string UserSettingsPath { get; set; }
        bool IsDemo { get; set; }
        string RootPath { get; set; }
        bool IsRegAppTable { get; set; }
        string LibName { get; set; }
        string UserSettingsFileName { get; set; }
        string UserConfigFileName { get; set; }
    }

    /// <summary> class Project contains project properties </summary>
    public class PluginManager1 : IPluginManager1
    {
        public string Copyright;
        public string TemplateFullPath; //templates path for work items
        public string Version;

        public bool IsRegAppTable { get; set; }
        public string WorkPath { get; set; } //work path for work items
        public string RootPath { get; set; } //root path of placement of assembly
        public bool IsDemo { get; set; }
        public string LibName { get; set; }
        public string UserSettingsPath { get; set; } //path to user xml settings
        public string UserConfigFileName { get; set; } //"User.config"
        public string UserSettingsFileName { get; set; }
        public string LayoutFiltersFileName { get; set; }

        public string Name { get; set; }
        public string NameMsg { get; set; }
        public bool IsComposed { get; set; }

        public PluginManager1()
        {
            NameMsg = "\n\n";
        }

        public void Initialize(Assembly assembly, string projectName)
        {
            Name = String.IsNullOrEmpty(projectName) ? assembly.GetName().Name : projectName;
            WorkPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\IntelliDesk";

            if (!Directory.Exists(WorkPath))
            {
                Directory.CreateDirectory(WorkPath);
                //ProjectManager.TemplateFullPath = Register.GetAutoCADKey();
            }

            RootPath = Path.GetDirectoryName(assembly.Location) + "\\";
            NameMsg = "\n\n" + Name + ": "; //.PadRight(7,Convert.ToChar("."))
            Version = "(v" + assembly.GetName().Version + ")";
            UserSettingsPath = GetUserPath();
        }

        private string GetUserPath()
        {
            string userPath;
            if (RootPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
            {
                userPath = RootPath;
            }
            else
            {
                userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name + "\\");
                if (!Directory.Exists(UserSettingsPath))
                {
                    Directory.CreateDirectory(userPath);
                }
            }
            return userPath;
        }

        public bool Compose(object part, Assembly assembly)
        {
            var result = false;
            // An aggregate catalog can contain one or more types of catalog
            var catalog = new AggregateCatalog();
            var url = new Uri(assembly.CodeBase); //Assembly.GetExecutingAssembly()
            var path = Path.GetDirectoryName(url.LocalPath);

            catalog.Catalogs.Add(new DirectoryCatalog(path));
            var container = new CompositionContainer(catalog);

            try
            {
                //Creates composable parts.
                container.ComposeParts(part);
                result = true;
            }
            catch (CompositionException)
            {
                //UIManager.WriteMessage("MEF is not working.\nWill apply the Compose by without MEF\nError message: " + ex);
            }

            //if (!Database.Exists("AcadNet"))
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<AcadNetContext>());

            return result;
        }

        public static bool IsProcessRunning(string name)
        {
            return Process.GetProcessesByName(name).Length > 0;
        }
    }
}