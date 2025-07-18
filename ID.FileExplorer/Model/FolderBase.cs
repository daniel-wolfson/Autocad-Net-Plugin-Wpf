using ID.Infrastructure;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FileExplorer.Model
{
    public abstract class FolderBase : FileBase, IFolder, IFolderCheck
    {
        protected const string SearchAllWildChar = "*";
        protected object LockObj = new object();
        public static bool IsReloading;
        public static bool IsCleaning;
        protected const int chunk = 50;

        #region IFolder properties

        public bool CanReloading()
        {
            return !this.IsFolderLoaded || !this.IsFileLoaded || IsReloading || IsCleaning;
        }

        public override string Title
        {
            get
            {
                return title.IsNullOrEmpty() ? this.Name : title;
            }
            set
            {
                SetProperty(ref title, value, "Title");
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                SetProperty(ref _isLoading, value, "IsLoading");
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                SetProperty(ref _isExpanded, value, "IsExpanded");
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value, "IsSelected");
            }
        }

        private bool? _isChecked;
        public override bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (SetProperty(ref _isChecked, value, "IsChecked"))
                {
                    //CheckParent(this, _isChecked);
                    CheckChildren(this, _isChecked);
                    RaiseCheckedChanged(this);
                }
            }
        }

        private bool _isCheckVisible = true;
        /// <summary> Is tree item  checkbox visible </summary>
        public bool IsCheckVisible
        {
            get { return _isCheckVisible; }
            set { SetProperty(ref _isCheckVisible, value, "IsCheckVisible"); }
        }

        public bool IsCanceled
        {
            get;
            protected set;
        }

        private IFolder virtualParent;
        public IFolder VirtualParent
        {
            get { return virtualParent; }
            set { SetProperty(ref virtualParent, value, "VirtualParent"); }
        }

        #endregion

        protected FolderBase(string path, IFolder parent)
        {
            this.FullPath = path;
            this.Parent = parent;

            //The root item's parent is null for the constructor
            if (!parent.IsNull() && this.Parent.IsChecked == true)
                this.SetChecked(this.Parent.IsChecked);
            fileAttr = FileAttributes.Directory;
        }

        protected FolderBase()
        {
            fileAttr = FileAttributes.Directory;
        }

        private ObservableCollection<IFolder> _folders = new ObservableCollection<IFolder>();
        /// <summary> Sub _folders </summary>
        public ObservableCollection<IFolder> Folders
        {
            get { return _folders; }
            set { _folders = value; }
        }

        private ObservableCollection<IFile> _files = new ObservableCollection<IFile>();
        /// <summary> Sub _files </summary>
        public ObservableCollection<IFile> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        private readonly ObservableCollection<IFile> _items = new ObservableCollection<IFile>();
        /// <summary>
        /// Sub _folders and _files
        /// </summary>
        public ObservableCollection<IFile> Items
        {
            get { return _items; }
        }

        /// <summary> Is children folder loaded </summary>
        public bool IsFolderLoaded { get; set; }

        private static IFolder _baseFolder;
        public static IFolder BaseFolder => _baseFolder
               ?? (_baseFolder = Plugin.GetService<IFolder>("BaseFolder"));

        /// <summary> Is children _files loaded </summary>
        public bool IsFileLoaded { get; set; }

        public bool IsFileShortcut { get; set; }

        /// <summary> Only support for all checked or no checked by parent, UI operations by user  </summary>
        /// <param name="folder"></param>
        /// <param name="isChecked"></param>
        public void CheckChildren(IFolder folder, bool? isChecked)
        {
            if (folder.IsNull() || !isChecked.HasValue)
            {
                return;
            }

            ///Use property better than method is for lazy load
            foreach (IFile file in folder.Files)
            {
                file.SetChecked(isChecked);
            }

            foreach (IFolder folderItem in folder.Folders)
            {
                folderItem.SetChecked(isChecked);
            }

            foreach (IFolder folderItem in folder.Folders)
            {
                CheckChildren(folderItem, isChecked);
            }
        }

        /// <summary> Get all checked _items  </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public IEnumerable<IFile> GetCheckedItems(IFolder folder)
        {
            IEnumerable<IFile> result = new IFile[0];
            if (folder.IsNull() || folder.IsChecked == false || !folder.IsEnabled
                || ((FolderBase)folder).fileAttr == FileAttributes.ReadOnly)
            {
                return result;
            }

            //The root folder's parent is itself so filter these folder _items
            if (folder.IsChecked == true && folder.Parent != folder && folder.IsEnabled && !string.IsNullOrEmpty(folder.FullPath))
            {
                result = result.Union(new IFile[] { folder });
            }
            else
            {
                foreach (var item in folder.Folders.Where(f => f.IsEnabled && !string.IsNullOrEmpty(f.FullPath))) //folder.Items
                {
                    if (item.IsChecked == true)
                    {
                        result = result.Union(new IFile[] { item });
                    }
                    else if ((item.IsChecked == null) && (item is IFolder))
                    {
                        IFolder subFolder = item as IFolder;
                        if (!subFolder.IsNull())
                        {
                            result = result.Union((item as IFolderCheck).GetCheckedItems(subFolder));
                        }
                    }
                }
            }

            return result;
        }

        public virtual void Cancel()
        {
            this.IsCanceled = true;
            this.IsLoading = false;
            this.Clear();
        }

        /// <summary> Clear _files </summary>
        protected void Clear()
        {
            if (!this.IsFolderLoaded)
            {
                foreach (var item in this.Folders)
                {
                    item.Dispose();
                    this.Items.Remove(item);
                }
                this.Folders.Clear();
                this.AddPlaceHolder();
            }

            if (!this.IsFileLoaded)
            {
                foreach (var item in this.Files)
                {
                    this.Items.Remove(item);
                }
                this.Files.Clear();
            }
        }

        protected abstract void AddPlaceHolder();

        protected IEnumerable<T> AddItemsByChunk<T>(IEnumerable<T> source) where T : IFile
        {
            // , params IList[] destinations || destinations.IsNull()
            List<T> results = null;

            if (source.IsNullOrEmpty())
                return results;

            int index = 0;
            int getCount = chunk;
            while (getCount > 0)
            {
                var chunkItems = source.Skip(index++ * chunk).Take(chunk);
                getCount = chunkItems.Count();
                if (IsCanceled)
                {
                    RunOnUIThread(Clear);
                    return results;
                }

                if (getCount == 0) break;

                RunOnUIThread(() =>
                {
                    results = new List<T>();
                    foreach (var item in chunkItems)
                    {
                        if (IsCanceled)
                        {
                            return;
                        }
                        //foreach (var list in destinations)
                        //{
                        //    list.Add(item);
                        //}
                        results.Add(item);
                    }

                    //if (destinations.FirstOrDefault() != null)
                    //{
                    //}
                });
            }
            return results;
        }

        #region IFolderAsync

        public virtual async Task<IFolder> GetChildrenAsync(bool isRecursive = false)
        {
            var taskGetFolders = Task.Run(async () => await this.GetFoldersAsync());
            var taskGetFiles = Task.Run(async () => await this.GetFilesAsync());
            await Task.WhenAll(taskGetFolders, taskGetFiles);

            if (isRecursive)
            {
                List<Task<IFolder>> tasks = new List<Task<IFolder>>();
                this.Folders.ForEach(childfolder =>
                {
                    if (childfolder.CanReloading())
                        tasks.Add(Task.Run(() => childfolder.GetChildrenAsync(true)));
                });
                await Task.WhenAll(tasks);
            }

            this.Items.Clear();
            this.Items.AddRange(this.Folders.Concat(this.Files));
            return this;
        }

        public virtual async Task<IFolder> LoadChildrenAsync(bool isRecursive = false)
        {
            //Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { IsLoading = true; })).Wait();
            //Dispatcher.CurrentDispatcher.Invoke(new Action(() => { IsLoading = true; }), DispatcherPriority.ContextIdle, null);
            RunOnUIThreadAsync(() =>
            {
                IsLoading = true;
                Mouse.OverrideCursor = Cursors.Wait;
            });

            RunOnUIThread(async () =>
            {
                await this.GetFoldersAsync(isRecursive);
                await this.GetFilesAsync();

                if (Items.Count > 0)
                    Items.Clear();

                if (IsCleaning)
                    this.Folders.AddRange(this.Folders.Select(x => { x.IsChecked = false; return x; }));

                Items.AddRange(this.Folders);
                Items.AddRange(this.Files);

                IsLoading = false;
            });

            RunOnUIThreadAsync(() =>
            {
                IsLoading = false;
                Mouse.OverrideCursor = null;
            });

            return await Task.FromResult(this);
        }

        public virtual async Task<IFolder> LoadFoldersByPathAsync(List<string> pathList = null, Action<IFolder> foundFolderCallBack = null)
        {
            if (this.CanReloading())
                await this.GetChildrenAsync();

            if (pathList == null || !pathList.Any()) return this;

            var foundFolder = this.Folders.FirstOrDefault(x => x.FullPath.Contains(pathList[0].ToUpper()));
            if (foundFolder != null)
            {
                if (foundFolder.CanReloading())
                    await ((FolderBase)foundFolder).LoadChildrenAsync();
                pathList.RemoveAt(0);

                if (pathList.Count > 0)
                {
                    foundFolder.IsExpanded = true;
                    await foundFolder.LoadFoldersByPathAsync(pathList, foundFolderCallBack);
                }
                else
                {
                    foundFolder.IsSelected = true;
                    foundFolder.IsExpanded = true;

                    if (foundFolderCallBack != null)
                        foundFolderCallBack(foundFolder);
                }
            }
            return foundFolder;
        }

        public virtual async Task<IFolder> SetFoldersAsync(string[] folderPaths)
        {
            this.Folders.Clear();

            if (folderPaths.Length == 0)
                folderPaths = new string[] { "C:\\" };

            List<Task<IFolder>> tasks = new List<Task<IFolder>>();
            foreach (var includeFolder in folderPaths)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var folder = await new LocalFolder(includeFolder, null).GetChildrenAsync();
                    this.Folders.Add(folder);
                    return folder;
                }));
            }
            await Task.WhenAll(tasks);

            PluginSettings.IncludeFolders.Clear();
            PluginSettings.IncludeFolders.AddRange(folderPaths);

            Plugin.RegisterInstance<IFolder>(this, "BaseFolder");

            //Parallel.ForEach(appSettings.IncludeFolders, async f =>
            //{
            //    var task = await new LocalFolder(f, null).GetChildrenAsync();
            //    baseFolder.Folders.Add(task);
            //});

            //await Task.WhenAll(appSettings.IncludeFolders
            //        .Select(async i =>
            //        {
            //            var t = await new LocalFolder(i, null).GetChildrenAsync();
            //            baseFolder.Folders.Add(t);
            //        }));

            return this;
        }

        public abstract Task<IFolder> GetFoldersAsync(bool isRecursive = false);

        public abstract Task<IFile> GetFilesAsync();

        public async void GetItemAsync(string path, Action<IFile> callback, bool isRecursive = true)
        {
            if (path.IsNullOrEmpty() ||
                (!this.FullPath.IsNullOrEmpty() && !path.StartsWith(FullPath)))
            {
                if (!callback.IsNull())
                    callback(null);
                return;
            }

            if (path == this.FullPath)
            {
                if (!callback.IsNull())
                    callback(this);
                return;
            }

            IFile result = null;
            if (!isRecursive)
            {
                var items = await this.LoadChildrenAsync();
                if (items.Files.Any())
                    result = items.Files.FirstOrDefault(f => f.FullPath == path);

                if (!callback.IsNull())
                    callback(result);
                return;
            }

            ///Get folder recursively
            const string pathFormat = "{0}{1}";
            const StringSplitOptions splitOpt = StringSplitOptions.RemoveEmptyEntries;
            char pathSepChar = Path.DirectorySeparatorChar;
            char[] pathSepChars = new[] { pathSepChar };

            string[] parts = null;
            if (this.FullPath.IsNullOrEmpty())
            {
                parts = path.Split(pathSepChars, splitOpt);
            }
            else if (this.FullPath == pathSepChar.ToString())
            {
                ///Remote root folder full path is \
                parts = path.Substring(path.IndexOf(this.FullPath) + 1).Split(pathSepChars, splitOpt);
            }
            else
            {
                parts = path.Replace(this.FullPath, string.Empty).Split(pathSepChars, splitOpt);
            }

            if (parts.IsNullOrEmpty())
            {
                if (!callback.IsNull())
                {
                    callback(null);
                }
                return;
            }

            string newPath = FullPath;

            if (newPath.IsNullOrEmpty())
            {
                ///The driver path,end with '\'
                newPath = string.Format(pathFormat, parts[0], pathSepChar);
            }
            else
            {
                newPath = Path.Combine(newPath, parts[0]);
            }

            var children = await this.LoadChildrenAsync();

            IFile item = children.Files.FirstOrDefault(f => f.FullPath == newPath);
            if (!item.IsNull())
            {
                if (item.FullPath == path)
                {
                    if (!callback.IsNull())
                    {
                        callback(item);
                        return;
                    }
                }

                if (item is IFolder)
                {
                    (item as IFolder).GetItemAsync(path, callback, isRecursive);
                }
            }
            else if (!callback.IsNull())
            {
                callback(null);
                return;
            }
        }

        protected static IEnumerable<T> SetFolderOrder<T>(IEnumerable<T> list, bool isAsc = true) where T : IFile
        {
            if (list.IsNullOrEmpty())
            {
                return list;
            }

            if (isAsc)
            {
                return list.OrderBy(item => item.Name);
            }
            else
            {
                return list.OrderByDescending(item => item.Name);
            }
        }

        protected static IEnumerable<T> SetFileOrder<T>(IEnumerable<T> list, bool isAsc = true) where T : IFile
        {
            if (list.IsNullOrEmpty())
            {
                return list;
            }

            if (isAsc)
            {
                return list.OrderBy(item => item.Name);//.ThenBy(item => item.Name.Length);
            }
            else
            {
                return list.OrderByDescending(item => item.Name);//.ThenByDescending(item => item.Name.Length);
            }
        }

        #endregion

        protected override void OnDisposing(bool isDisposing)
        {
            this.Cancel();
            foreach (var item in this.Folders)
            {
                //item.Dispose();
            }
            this.Folders.Clear();
            this.Files.Clear();
            this.Items.Clear();
            this.Parent = null;
        }
    }
}
