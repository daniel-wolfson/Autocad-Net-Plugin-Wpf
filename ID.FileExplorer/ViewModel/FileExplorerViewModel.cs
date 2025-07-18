using Autodesk.AutoCAD.ApplicationServices;
using FileExplorer.Factory;
using FileExplorer.Helper;
using FileExplorer.Model;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Files = ID.Infrastructure.Files;

namespace FileExplorer.ViewModel
{
    public class FileExplorerViewModel : SortOrderViewModel, IFileExplorerViewModel
    {
        private readonly object lockObj = new object();

        #region <Properties>
        private ExplorerFactoryBase RootFactory { get; set; }
        private ObservableCollection<IFolder> _items = new ObservableCollection<IFolder>();
        private ObservableCollection<IFile> _fileItems = new ObservableCollection<IFile>();
        private IFolder _computer;
        private IFolder _baseFolder;
        private IFolder _currentFolder;
        private IFolder _selectedFolder;
        private IFile _currentFile;
        private ISearchViewModel _searchViewModel;
        private bool _isLoading;
        private bool _showAll;

        public ObservableCollection<IFolder> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value, "Items"); }
        }

        public ObservableCollection<IFile> FileItems
        {
            get { return _fileItems; }
            set { SetProperty(ref _fileItems, value, "FileItems"); }
        }

        public bool SelectSaveEnable { get; set; }
        public IFolder Computer
        {
            get
            {
                if (_computer == null)
                    _computer = this.LoadLocalExplorer();
                return _computer;

            }
            set { SetProperty(ref _computer, value, "Computer"); }
        }
        public IFolder BaseFolder
        {
            get
            {
                return _baseFolder ?? (_baseFolder = Plugin.GetService<IFolder>("BaseFolder"));
            }
            set
            {
                SetProperty(ref _baseFolder, value, "BaseFolder");
                if (_baseFolder.Folders.Any())
                    CurrentFolder = _baseFolder.Folders.First();
            }
        }
        public IFolder CurrentFolder
        {
            get
            {
                return _currentFolder ?? (_currentFolder = BaseFolder.Folders.FirstOrDefault());
            }
            set
            {
                if (SetProperty(ref _currentFolder, value, "CurrentFolder"))
                {
                    if (value != null)
                        PluginSettings.CurrentFolder = value.FullPath;
                    else if (BaseFolder.Folders.Any())
                        PluginSettings.CurrentFolder = BaseFolder.Folders.First().FullPath;
                    else
                        PluginSettings.CurrentFolder = "C:\\";
                }
            }
        }
        public IFolder SelectedFolder
        {
            get
            {
                return _selectedFolder ?? (_selectedFolder = CurrentFolder);
            }
            set
            {
                if (SetProperty(ref _selectedFolder, value, "SelectedFolder"))
                {
                }
            }
        }
        public IFile CurrentFile
        {
            get { return _currentFile; }
            private set
            {
                SetProperty(ref _currentFile, value, "CurrentFile");
            }
        }
        public ISearchViewModel SearchViewModel
        {
            get { return _searchViewModel; }
            set { SetProperty(ref _searchViewModel, value, "SearchViewModel"); }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value, "IsLoading"); }
        }
        public bool ShowAll
        {
            get { return _showAll; }
            set { SetProperty(ref _showAll, value, "ShowAll"); }
        }
        #endregion

        #region <Commands>

        private ICommand _openFileCommand;
        private ICommand _sortFilesCommand;
        private ICommand _selectedFolderCommand;
        private ICommand _refreshFolderDetailsCommand;
        private ICommand _selectingFileCommand;

        public ICommand OpenFileCommand => _openFileCommand ??
               (_openFileCommand = new DelegateCommand<object>(ExecuteOpenFolderOrFile, CanExecuteFolderOrFile));

        public ICommand SortFilesCommand => _sortFilesCommand ??
               (_sortFilesCommand = new DelegateCommand<GridViewColumn>(ExecuteSortFiles, CanExecuteFolderOrFile));

        public ICommand SelectedFolderCommand => _selectedFolderCommand ??
               (_selectedFolderCommand = new DelegateCommand<IFolder>(ExecuteSelectedFolder));

        public ICommand RefreshFolderDetailsCommand => _refreshFolderDetailsCommand ??
               (_refreshFolderDetailsCommand = new DelegateCommand<IFolder>(ExecuteFolderDetailsRefresh, CanExecuteSelect));

        public ICommand SelectionFileCommand => _selectingFileCommand ??
               (_selectingFileCommand = new DelegateCommand<object>(ExecuteSelectionFile));

        private async void ExecuteExpand(object viewItem)
        {
            var item = viewItem as TreeViewItem;
            if (item == null)
                return;

            IFolder folder = item.DataContext as IFolder;
            if (folder.IsNull())
                return;

            await LoadFolderChildren(folder);
        }

        private void ExecuteSortFiles(GridViewColumn column)
        {
            var header = column.Header as GridViewColumnHeader;
            if (header == null || string.IsNullOrEmpty(header.Content.ToString())) return;
            header.Name = column.GetSortPropertyValue();

            SetSortOrder(column);
        }

        public void ExecuteSelectedFolder(IFolder folder)
        {
            SetCurrentFolder(folder);
        }

        private void ExecuteSelectionFile(object item)
        {
            FileDataInfoBase dataInfo = null;
            RunOnUIThread(() => { IsLoading = true; Mouse.OverrideCursor = Cursors.Wait; });

            if (item is IFile)
            {
                var file = item as IFile;
                if (Path.GetExtension(file.FullPath.ToLower()) == ".dwg")
                {
                    var doc = Documents.DocumentFind(file.FullPath);
                    dataInfo = new FileDataInfoBase();
                    //doc != null ? new FileDataInfo(doc) : new FileDataInfoBase();
                }
                else
                {
                    dataInfo = new FileDataInfoBase();
                }
                dataInfo.Type = file.TypeName;
                file.DataInfo = dataInfo;
                CurrentFile = file;
            }

            if (item is IFolder)
            {
                CurrentFile = null;
                var folder = item as IFolder;
                dataInfo = new FileDataInfoBase { Type = folder.TypeName };
                folder.DataInfo = dataInfo;
                CurrentFolder = folder;
            }

            RunOnUIThread(() => { IsLoading = false; Mouse.OverrideCursor = null; });
        }

        private void ExecuteOpenFolderOrFile(object item)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            NotifyArgs notifyResult = new ReadyNotifyArgs();
            Notifications.SendNotifyMessageAsync(new NotifyArgs(NotifyStatus.Loading));

            if (item is IFolder)
            {
                IFolder folder = item as IFolder;
                folder.IsExpanded = true;
                folder.IsSelected = true;
                SetCurrentFolder(folder);
            }
            else if (item is IFile)
            {
                IFile file = item as IFile;
                if (File.Exists(file.FullPath))
                {
                    try
                    {
                        if (file.FullPath.ToLower().Contains(".lnk"))
                        {
                            var realFilePath = Files.GetShortcutTargetFile(file.FullPath);
                            Process.Start(realFilePath);

                            var realFullPath = Path.GetDirectoryName(realFilePath);
                            if (realFullPath != null)
                            {
                                IFolder folder = new LocalFolder(new DirectoryInfo(realFullPath).FullName);
                                folder.IsExpanded = true;
                                folder.IsSelected = true;
                                SetCurrentFolder(folder);
                            }
                        }
                        else if (!file.FullPath.ToLower().Contains(".dwg"))
                        {
                            Process.Start(file.FullPath);
                            PluginSettings.CurrentFolder = Path.GetDirectoryName(Path.GetFullPath(file.FullPath));
                            PluginSettings.Save();
                        }
                        else
                        {
                            if (file.IsFileLoaded())
                            {
                                var doc = acadApp.DocumentManager.Cast<Document>()
                                    .FirstOrDefault(x => x.Name.ToLower() == file.FullPath.ToLower());
                                if (doc != null)
                                    acadApp.DocumentManager.MdiActiveDocument = doc;
                            }
                            else
                            {
                                DocumentCollectionEventHandler handler = null;
                                handler = (sender, args) =>
                                {
                                    FileDataInfo _dataInfo = new FileDataInfo(file.FullPath);
                                    _dataInfo.CADFileName =
                                        acadApp.DocumentManager.MdiActiveDocument.Database.OriginalFileName;
                                    _dataInfo.CreatedBy = Environment.UserName;
                                    _dataInfo.ModifiedBy = Environment.UserName;
                                    _dataInfo.DateCreated = DateTime.Now;
                                    _dataInfo.DateModified = DateTime.Now;
                                    _dataInfo.CoordSystem = CoordSystem.ITM;
                                    _dataInfo.Type = "Autocad Drawing";
                                    _dataInfo.Visible = true;
                                    _dataInfo.CADFileName = file.FullPath;
                                    file.DataInfo = _dataInfo;
                                    CurrentFile = file;
                                    acadApp.DocumentManager.DocumentActivated -= handler;
                                };
                                //WApplication.DocumentManager.DocumentActivated += handler;

                                Documents.DocumentAction(file.FullPath, DocumentOptions.OpenAndActive);

                                //acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("tilemode 1 ", true, false, false);
                                //acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("_mspace ", true, false, false);
                                //Task.Run(async () =>
                                //{
                                //    await acadApp.DocumentManager.ExecuteInCommandContextAsync(async data =>
                                //    {
                                //        Editor ed = acadApp.DocumentManager.CurrentDocument.Editor;
                                //        await ed.CommandAsync(CommandNames.XSwitchToModelspace);
                                //    }, null);
                                //});
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        notifyResult = new ErrorNotifyArgs(ex.ToStringMessage());
                        Plugin.Logger.Error("Exception:", ex);
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
                else
                {
                    notifyResult = new ErrorNotifyArgs("File not found");
                }
            }

            Mouse.OverrideCursor = null;
            Notifications.SendNotifyMessageAsync(notifyResult);
        }

        private bool CanExecuteSelect(object item)
        {
            return SelectSaveEnable;
        }

        private bool CanExecuteFolderOrFile(object item)
        {
            return !PluginSettings.Busy;
        }

        #endregion

        #region <ctor>

        public FileExplorerViewModel()
        {
            //SearchViewModel = new SearchViewModel((IPanel)this);
            UiDispatcher = Dispatcher.CurrentDispatcher;
            //CommandLine.UiDispatcher
        }

        #endregion

        #region <Methods>

        #region Methods: Check operation

        public IEnumerable<string> GetCheckedPaths()
        {
            IEnumerable<string> result = null;
            IEnumerable<IFile> checkedItems = null;
            //if (this.SearchViewModel.IsSearchEnabled) checkedItems = this.SearchViewModel.GetCheckedItems();

            var folderCheck = Computer as IFolderCheck;
            if (folderCheck != null)
                checkedItems = folderCheck.GetCheckedItems(Computer);

            if (checkedItems != null)
                result = checkedItems.Select(item => item.FullPath);
            return result;
        }

        IList<string> _checkedPaths;
        public IList<string> CheckedPaths
        {
            get
            {
                _checkedPaths = _checkedPaths ?? PluginSettings.IncludeFolders.Cast<string>().ToList();
                return _checkedPaths;
            }
            private set
            {
                if (!Equals(_checkedPaths, value))
                    _checkedPaths = value;
            }
        }

        public void SetCheckedPaths(IList<string> pathList, bool isChecked = true)
        {
            if (pathList.IsNullOrEmpty())
                return;

            this.CheckedPaths = pathList.OrderBy(item => item.Length).ToList();
            SetPathChecked(CheckedPaths, isChecked);
        }

        public void SetPathChecked(IEnumerable<string> list, bool isChecked = true)
        {
            if (list.IsNullOrEmpty() || this.Computer.IsNull()) //this.RootFolder.IsLoading
                return;

            foreach (string path in list)
            {
                this.Computer.GetItemAsync(path, folder =>
                {
                    if (!folder.IsNull())
                    {
                        folder.IsChecked = folder is LocalRootFolder ? (bool?)null : isChecked;
#if DEBUG
                        Plugin.Logger.Debug("/---- selected path:{0}", folder.FullPath);
#endif
                    }
                });
            }
        }

        #endregion

        public void SetCurrentFolder(IFolder folder)
        {
            this.RunOnUIThread(async () =>
            {
                ClearSortOptions();

                folder.IsSelected = true;
                this.CurrentFolder = folder;

                if (folder.CanReloading())
                    await folder.LoadChildrenAsync();

                ExecuteFolderDetailsRefresh(folder);
            });
        }

        private void ExecuteFolderDetailsRefresh(IFolder folder)
        {
            lock (lockObj)
            {
                try
                {
                    SelectedFolder = folder;

                    if (folder.VirtualParent != null && folder.VirtualParent.Name.Contains("Workitems"))
                    {
                        IEnumerable<LocalFile> realPaths = folder.Items.Select(x =>
                        {
                            var file = new LocalFile(
                                Files.GetShortcutTargetFile(x.FullPath),
                                new LocalFolder(Path.GetDirectoryName(Path.GetFullPath(x.FullPath))));
                            file.Title = Files.GetShortcutTargetFile(x.FullPath);
                            file.Name = Files.GetFileName(Files.GetShortcutTargetFile(x.FullPath));
                            return file;
                        });
                        this.FolderDetailsCollectionView = CollectionViewSource.GetDefaultView(realPaths);
                    }
                    else
                        this.FolderDetailsCollectionView = CollectionViewSource.GetDefaultView(folder.Items);

                    this.SetSortOrder(SortPropertyName, ListSortDirection.Ascending);
                    this.InvokeSortOrderCallback();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public async Task<IFolder> LoadFolderChildren(IFolder folder)
        {
            IFolder result = null;
            Mouse.OverrideCursor = Cursors.Wait;
            if (!folder.IsNull() && (this.CurrentFolder != folder || folder.CanReloading()))
                result = await folder.LoadChildrenAsync();
            SetPathChecked(CheckedPaths, ShowAll);
            Mouse.OverrideCursor = null;
            return result;
        }

        public bool IsExplorerInitialized
        {
            get { return this.RootFactory != null; }
        }

        internal IFolder InitialExplorer(ExplorerFactoryBase factory)
        {
            if (factory.IsNull())
                throw new ArgumentNullException("factory");

            IFolder root = null;
            RemoveExplorer();

            this.RootFactory = factory;
            this.RootFactory.GetRootFoldersAsync(folderItems =>
            {
                root = folderItems.OfType<LocalRootFolder>().FirstOrDefault();
            });

            return root;
        }

        public IFolder LoadLocalExplorer()
        {
            return this.InitialExplorer(new LocalExplorerFactory());
        }

        //public void LoadExplorerByJob(BackupJob backupJob)
        //{
        //    if (backupJob == null)
        //    {
        //        throw new ArgumentNullException("BackupJob");
        //    }

        //    ExplorerFactoryBase result = null;
        //    switch (backupJob.JobTarget)
        //    {
        //        case JobTarget.HardDisk:
        //        case JobTarget.LocalNetworkStorage:
        //        case JobTarget.RemovableDisk:
        //            if (backupJob.LastBackupFile == null ||
        //                string.IsNullOrEmpty(backupJob.LastBackupFile.NbixFilePath))
        //            {
        //                throw new ArgumentNullException("backupJob.LastBackupFile");
        //            }
        //            result = new JsonExplorerFactory(backupJob.LastBackupFile.NbixFilePath);
        //            break;

        //        case JobTarget.OpticalDisk:
        //            if (string.IsNullOrEmpty(backupJob.JobFilePath))
        //            {
        //                throw new ArgumentNullException(" backupJob.JobFilePath");
        //            }
        //            result = new CDRomExplorerFactory(backupJob.JobFilePath);
        //            break;

        //        case JobTarget.Online:
        //            result = new CloudExplorerFactory();
        //            break;

        //        default:
        //            result = new LocalExplorerFactory();
        //            break;
        //    }
        //    this.InitialExplorer(result);
        //}

        private void RemoveExplorer()
        {
            //foreach (var item in this.Items)
            //{
            //    //item.Dispose();
            //}

            if (Items.Count > 0)
                this.Items.Clear();
            Thread.Sleep(1000);
            this.RootFactory = null;
            this.Computer = null;
            this.FolderDetailsCollectionView = null;
            this.CurrentFolder = null;

            //this.SearchViewModel.UninitialSearch();
        }

        protected override void OnDisposing(bool isDisposing)
        {
            RemoveExplorer();
            this.SearchViewModel = null;
            base.OnDisposing(isDisposing);
        }

        #endregion
    }
}
