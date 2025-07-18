using FileExplorer.Model;
using FileExplorer.Shell;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FileExplorer.Factory
{
    class LocalExplorerFactory : ExplorerFactoryBase
    {
        public const string ComputerParseName = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
        public static DataSourceShell GetPcRootShellItem()
        {
            DataSourceShell pcShell = GetShellItem(ComputerParseName);
            return pcShell;
        }

        public override void GetRootFoldersAsync(Action<IEnumerable<IFolder>> callback)
        {
            ObservableCollection<IFolder> roots = new ObservableCollection<IFolder>();
            LocalRootFolder pcFolder = new LocalRootFolder();
            LocalVirtualFolder personFolder = null;

            roots.Add(pcFolder);

            IList<string> personPaths = new List<string>()
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"IntelliDesk")
                //Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                //Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            foreach (var path in personPaths)
            {
                DataSourceShell item = GetShellItem(path);
                if (!item.IsNull())
                {
                    using (item)
                    {
                        personFolder = new LocalVirtualFolder(item);
                        roots.Add(personFolder);
                    }
                    break;
                }
            }

            if (!personFolder.IsNull())
            {
                pcFolder.GetItemAsync(personFolder.FullPath, (item) =>
                {
                    if (!item.IsNull() && (item is IFolder))
                    {
                        personFolder.RealFolder = item as IFolder;
                        personFolder.IsExpanded = true;
                    }
                });
            }

            if (!callback.IsNull()) // && PluginSettings.ProjectExplorerIsActive
            {
                callback(roots);
            }
        }
    }
}
