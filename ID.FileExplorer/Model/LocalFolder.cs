using ID.Infrastructure;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileExplorer.Model
{
    public class LocalFolder : FolderBase
    {
        private static readonly List<string> LocalExcludedFolders = new List<string>();

        static LocalFolder()
        {
            var str = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string progPath = str.IsNullOrEmpty() ? string.Empty : str.ToLower();

            str = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string progX86Path = str.IsNullOrEmpty() ? string.Empty : str.ToLower();

            str = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string windPath = str.IsNullOrEmpty() ? string.Empty : str.ToLower();

            str = Environment.GetEnvironmentVariable("ProgramW6432");
            string progW6432 = str.IsNullOrEmpty() ? string.Empty : str.ToLower();

            if (!windPath.IsNullOrEmpty())
            {
                LocalExcludedFolders.Add(windPath);
            }

            if (!progPath.IsNullOrEmpty())
            {
                LocalExcludedFolders.Add(progPath);
            }

            if (!progW6432.IsNullOrEmpty() && !LocalExcludedFolders.Contains(progW6432))
            {
                LocalExcludedFolders.Add(progW6432);
            }

            if (!progX86Path.IsNullOrEmpty() && !LocalExcludedFolders.Contains(progX86Path))
            {
                LocalExcludedFolders.Add(progX86Path);
            }
        }

        const FileAttributes ExcludeFileAttributes = FileAttributes.Hidden;

        public static readonly IFolder PlackHolderItem = new LocalFolder();

        protected LocalFolder()
        {
            Name = "Loading...";
        }

        public LocalFolder(string path, IFolder parent = null)
            : base(path, parent)
        {
            // CD-ROM is not existed Virtual folder is not existed
            // if (!this.FullPath.IsNullOrEmpty() && Directory.Exists(this.FullPath))
            if (this.FullPath.IsNullOrEmpty())
                return;

            this.AddPlaceHolder();
            this.IsEnabled = !LocalExcludedFolders.Contains(path.ToLower());
            if (!this.Parent.IsNull() && !this.Parent.IsEnabled)
            {
                this.IsEnabled = this.Parent.IsEnabled;
            }

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(this.FullPath);
                if (!dirInfo.IsNull())
                {
                    this.fileAttr = dirInfo.Attributes;
                    this.Name = dirInfo.Name;
                    this.LastModifyTime = dirInfo.LastWriteTime;
                }
                // Pre-load network driver icon else will block UI
                var icon = this.Icon;
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(LocalFile)} Local folder constructor:{0}", ex);
            }
        }

        #region Methods

        public override async Task<IFolder> GetFoldersAsync(bool isRecursive = false)
        {
            IEnumerable<IFolder> folders = await SearchFolders();

            var projectFolders = PluginSettings.IncludeFolders.Where(Directory.Exists).ToList();
            if (!PluginSettings.ShowAllFolders && projectFolders.Any())
            {
                folders = folders.Where(x => projectFolders.Any(x.FullPath.Contains))
                    .Select(item =>
                    {
                        item.IsChecked = !PluginSettings.ShowAllFolders;
                        return item;
                    });
            }
            else if (!IsCleaning && projectFolders.Any())
            {
                folders = folders.Select(item =>
                {
                    item.IsChecked = projectFolders.Any(x => item.FullPath.Contains(x));
                    return item;
                });
            }
            else
                folders = folders.Select(item => { item.SetChecked(false); return item; });

            //folders = AddItemsByChunk(folders);
            IsFolderLoaded = true;

            var childfolders = folders as IList<IFolder> ?? folders.ToList();
            if (isRecursive)
                foreach (var childfolder in childfolders)
                {
                    if (childfolder.CanReloading())
                        await childfolder.GetFoldersAsync(true);
                }

            this.Folders.Clear();
            this.Folders.AddRange(childfolders);

            return this;
        }

        public override async Task<IFile> GetFilesAsync()
        {
            IEnumerable<IFile> files = SearchFiles();
            IsFileLoaded = true;

            this.Files.Clear();
            this.Files.AddRange(files);

            //Files = AddItemsByChunk(files);
            return await Task.FromResult(this);
        }

        private IEnumerable<IFile> SearchFiles(string searchPattern = SearchAllWildChar)
        {
            IEnumerable<IFile> result = new IFile[0];
            try
            {
                ///Check is folder is existed before query
                ///CD-ROM will block query for a while without check existed
                if (Directory.Exists(this.FullPath))
                {
                    string[] files = Directory.GetFiles(this.FullPath, searchPattern);
                    result = files.Where(item => (File.GetAttributes(item) & ExcludeFileAttributes) == 0)
                                  .Select(item => new LocalFile(item, this));
                    result = SetFileOrder(result);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                ///Access denied exception
                Plugin.Logger.Error($"{nameof(LocalFile)} Access denied exception:{{0}}", ex);
            }

            return result;
        }

        protected sealed override void AddPlaceHolder()
        {
            if (this != PlackHolderItem && !this.Folders.Contains(PlackHolderItem))
                this.Folders.Add(PlackHolderItem);
        }

        public override object Clone()
        {
            LocalFolder folder = new LocalFolder();
            CloneMembers(folder);
            return folder;
        }

        #endregion

        private async Task<IEnumerable<IFolder>> SearchFolders(string searchPattern = SearchAllWildChar)
        {
            IEnumerable<IFolder> result = new IFolder[0];
            try
            {
                ///Check is folder is existed before query
                ///CD-ROM will block query for a while without check existed
                if (Directory.Exists(this.FullPath))
                {
                    var dirs = Directory.GetDirectories(FullPath, searchPattern);
                    if (dirs.Any())
                    {
                        result = dirs.Where(item => (File.GetAttributes(item) & ExcludeFileAttributes) == 0)
                            .Select(item => new LocalFolder(item, this));

                        result = SetFolderOrder(result);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Plugin.Logger.Error($"{nameof(LocalFile)} Access denied exception:{{0}}", ex);
            }

            return await Task.FromResult(result);

        }
    }
}
