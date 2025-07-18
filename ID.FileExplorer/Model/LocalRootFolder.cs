using FileExplorer.Factory;
using FileExplorer.Helper;
using FileExplorer.Shell;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileExplorer.Model
{
    public class LocalRootFolder : LocalFolder
    {
        DriverWatcher _driverWatcher;

        public LocalRootFolder()
            : base(string.Empty, null)
        {
            using (DataSourceShell shellItem = LocalExplorerFactory.GetPcRootShellItem())
            {
                this.Name = shellItem.DisplayName;
                this.Icon = shellItem.Icon;
            }

            this.IsCheckVisible = false;
            this.IsExpanded = true;
            this.IsSelected = true;
            this.Parent = this;
        }

        public override async Task<IFolder> GetFoldersAsync(bool isRecursive = false)
        {
            var drivers = DriverWatcher.GetDrivers();

            if (!PluginSettings.ShowAllFolders && PluginSettings.IncludeFolders.Count > 0)
            {
                var projectDrivers = PluginSettings.IncludeFolders.Cast<string>().Where(Directory.Exists).ToList();
                if (!projectDrivers.Any())
                {
                    projectDrivers = projectDrivers.Select(Path.GetPathRoot).Distinct().ToList();
                    drivers = drivers.Where(d => projectDrivers.Any(drv => drv == d.Name)).ToList();
                }
            }
            else if (!IsFolderLoaded)
            {
                _driverWatcher = new DriverWatcher();
                _driverWatcher.Start(drivers.Select(item => item.Name).ToList());
                _driverWatcher.DriverChanged += OnDriverChanged;
            }

            IEnumerable<IFolder> driversList = drivers.Select(item => new LocalDriver(item.Name, this));
            //driversList = AddItemsByChunk(driversList); //, this.Folders, this.Items

            this.Folders.Clear();
            this.Folders.AddRange(driversList);

            return await Task.FromResult(this);
        }

        void OnDriverChanged(List<string> drives, bool isAdd)
        {
            if (drives.IsNullOrEmpty())
                return;

            RunOnUIThreadAsync(() =>
            {
                if (isAdd)
                {
                    foreach (var item in drives)
                    {
                        this.Folders.Add(new LocalDriver(item, this));
                    }
                }
                else
                {
                    foreach (var item in drives)
                    {
                        var driver = this.Folders.FirstOrDefault(d => d.Name == item);
                        if (!driver.IsNull())
                        {
                            driver.Dispose();
                            this.Folders.Remove(driver);
                        }
                    }
                }
            });
        }

        protected override void OnDisposing(bool isDisposing)
        {
            base.OnDisposing(isDisposing);
            if (!_driverWatcher.IsNull())
            {
                _driverWatcher.DriverChanged -= OnDriverChanged;
                _driverWatcher.Stop();
            }
        }
    }
}
