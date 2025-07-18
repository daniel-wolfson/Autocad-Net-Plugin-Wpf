using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.ViewModels;
using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace Intellidesk.AcadNet.Model
{
    public class SearchElementViewModel : BaseViewModelElement, ISearch, ISearchViewModel
    {
        protected IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();
        public new IBasePanelContext Parent { get; set; }

        #region <Properties>

        private string searchKeyword = string.Empty;
        private bool _isSearchEnabled = false;
        private string _messageInfo = "..."; //string.Empty
        private bool isSearching = false;
        private int imageCount = 0;
        private int videoCount = 0;
        private int musicCount = 0;
        private int documentCount = 0;
        private bool? isChecked = false;

        public string SearchKeyword
        {
            get { return searchKeyword; }
            set
            {
                if (SetProperty(ref searchKeyword, value, "SearchKeyword", "IsSearchEnabled"))
                {
                    if (!string.IsNullOrEmpty(this.searchKeyword)) // && Files.IsValidFilename(searchKeyword)
                    {
                        this.IsSearching = true;
                        StartSearchTimer();
                    }
                    else
                    {
                        this.Cancel();
                        this.IsSearching = false;
                        this.IsChecked = false;
                        this.OnPropertyChanged("IsSearchCompleted");
                    }
                }
            }
        }
        public bool IsSearchEnabled
        {
            get
            {
                MessageInfo = "";
                bool res = true;

                if (string.IsNullOrEmpty(SearchKeyword))
                {
                    MessageInfo = "SearchKeyword empty";
                    res = false;
                }

                //if (!Files.IsValidFilename(searchKeyword))
                //{
                //    MessageInfo = "not valid";
                //    res = false;
                //}
                return res;
            }
            set { SetProperty(ref _isSearchEnabled, value, "IsSearchEnabled"); }
        }
        public bool IsMessageInfoVisible => !IsSearchEnabled;

        public bool IsSearchCompleted
        {
            get
            {
                bool result = IsSearchEnabled && !this.IsSearching;
                return result;
            }
        }

        public bool IsSearchIncludedSubDir
        {
            get { return PluginSettings.SearchIncludeSubdir; }
            set { PluginSettings.SearchIncludeSubdir = value; }
        }

        public string MessageInfo
        {
            get { return _messageInfo; }
            set { SetProperty(ref _messageInfo, value, "MessageInfo"); }
        }
        public bool IsSearching
        {
            get { return isSearching; }
            set
            {
                if (SetProperty(ref isSearching, value, "IsSearching", "IsSearchCompleted")) // (Set(ref isSearching, value))
                {
                    //this.SetSearchHint();
                }
            }
        }
        public int AllCount
        {
            get { return this.Items.Count; }
        }
        public int ImageCount
        {
            get { return imageCount; }
            set
            {
                SetProperty(ref imageCount, value, "ImageCount");
            }
        }
        public int VideoCount
        {
            get { return videoCount; }
            set
            {
                SetProperty(ref videoCount, value, "VideoCount");
            }
        }
        public int MusicCount
        {
            get { return musicCount; }
            set
            {
                SetProperty(ref musicCount, value, "MusicCount");
            }
        }
        public int DocumentCount
        {
            get { return documentCount; }
            set
            {
                SetProperty(ref documentCount, value, "DocumentCount");
            }
        }
        public bool? IsChecked
        {
            get { return isChecked; }
            set
            {
                if (SetProperty(ref isChecked, value, "IsChecked"))
                    SetIsCheckedAsync();
            }
        }
        // IPanelViewModel Parent { get; set; }

        protected IFolder RootItem { get; set; }
        protected bool IsCanceled { get; set; }
        protected ObservableCollection<IFile> Items { get; } = new ObservableCollection<IFile>();
        protected ObservableCollection<string> SearchHistoryItems { get; } = new ObservableCollection<string>();
        public StringBuilder ActionMessage { get; } = new StringBuilder();

        public ICommand SearchClearCommand => new DelegateCommand<object>(ExecuteSearchClear);

        public ICommand SearchCommand => new DelegateCommand<object>(ExecuteSearch);

        #endregion

        public SearchElementViewModel()
        {
            //this.FolderDetailsCollectionView = CollectionViewSource.GetDefaultView(this.Items);
            //this.SetSortOrder(SortPropertyName, ListSortDirection.Ascending);
        }

        #region <Methods>
        private bool CanExecuteByBusy(object item)
        {
            return !PluginSettings.Busy;
        }
        private void ExecuteSearchClear(object value)
        {
            SearchKeyword = string.Empty;
        }
        private void ExecuteSearch(object value)
        {
            if (!IsSearchEnabled) return;

            InfraManager.RunOnUIThread(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Notifications.DisplayNotifyMessage(NotifyStatus.Working);
            });

            InfraManager.DelayAction(200, async () =>
            {
                //var commandArgs = await Parent.ExecuteCommand(new CommandArgs(null, "Search", SearchKeyword));
                //if (commandArgs.CancelToken.IsCancellationRequested)
                //{
                //}
            });
        }
        private void SetSearchHint()
        {
            OnPropertyChanged("AllCount");
            string searchDone = "Search Done";
            if (this.Items.IsNullOrEmpty())
                MessageInfo = searchDone + " " + "None content";
            else
                MessageInfo = searchDone + " " + "You can select";
        }
        private void SetIsCheckedAsync()
        {
            Action action = () =>
            {
                //May be is searching         
                if (this.IsChecked == true || this.IsChecked == false)
                {
                    var temp = this.Items.ToList();
                    foreach (var item in temp)
                    {
                        item.IsChecked = this.IsChecked;
                    }
                }
            };
            action.BeginInvoke((ar) => { action.EndInvoke(ar); }, action);
        }
        public void InitialSearch(IFolder rootItem)
        {
            UninitialSearch();
            if (!rootItem.IsNull())
                this.RootItem = rootItem;
        }
        public void UninitialSearch()
        {
            this.SearchKeyword = string.Empty;
            this.Cancel();
            this.IsSearching = false;
            this.IsChecked = false;
            if (!this.RootItem.IsNull())
            {
                this.RootItem = null;
            }
        }
        public IEnumerable<IFile> GetCheckedItems()
        {
            return this.Items.Where(item => item.IsChecked != false);
        }
        #endregion

        #region <Timer>

        TimeSpan interval = TimeSpan.FromSeconds(1);
        DispatcherTimer timer = null;

        private void StartSearchTimer()
        {
            if (timer.IsNull())
            {
                timer = new DispatcherTimer(interval, DispatcherPriority.Normal, (sender, e) =>
                {
                    if (this.RootItem.IsNull())
                    {
                        this.StopSearchTimer();
                        return;
                    }

                    if (this.SearchKeyword.IsNullOrEmpty())
                    {
                        this.IsSearching = false;
                        this.StopSearchTimer();
                        return;
                    }

                    if (this.RootItem.IsLoading)
                    {
#if DEBUG
                        Plugin.Logger.Debug("--- Searching timer, current folder is loading----");
#endif
                        //this.IsSearching = true;
                        //return;
                    }
                    else
                    {
                        this.Search(this.SearchKeyword);
                        this.StopSearchTimer();
                    }

                }, Dispatcher.CurrentDispatcher);
            }

            if (!timer.IsEnabled)
            {
                timer.Start();
            }
            this.IsSearching = true;
        }

        private void StopSearchTimer()
        {
            if (!timer.IsNull() && timer.IsEnabled)
            {
                timer.Stop();
            }
        }

        private void RestartSearchTimer()
        {
            this.StopSearchTimer();
            this.Cancel();
            this.StartSearchTimer();
        }

        #endregion

        #region <ISearch>

        public void Cancel()
        {
            this.IsCanceled = true;
            this.oldKeyword = string.Empty;
            this.regPattern = string.Empty;
            this.Items.Clear();
            this.searchStateList.Clear();
            this.MusicCount = 0;
            this.VideoCount = 0;
            this.ImageCount = 0;
            this.DocumentCount = 0;
        }

        string oldKeyword = string.Empty;
        string regPattern = string.Empty;

        public void Search(string newKeyword)
        {
            this.Cancel();
            if (RootItem == null || oldKeyword == newKeyword)
            {
                this.IsSearching = false;
                return;
            }

            this.IsCanceled = false;
            if (newKeyword.IsNullOrEmpty())
            {
                this.oldKeyword = newKeyword;
                this.IsSearching = false;
                return;
            }

            oldKeyword = newKeyword;
            regPattern = GetRegexPattern(newKeyword);
            SearchFolder(RootItem);
        }

        object lockObj = new object();
        IList<IFolder> searchStateList = new List<IFolder>();

        private void SearchFolder(IFolder searchFolder)
        {
            if (searchFolder.IsNull() || regPattern.IsNullOrEmpty())
            {
                return;
            }

            lock (lockObj)
            {
                if (!searchStateList.Contains(searchFolder))
                {
                    searchStateList.Add(searchFolder);
                    IsSearching = true;
                }
            }

            if (Thread.CurrentThread.IsBackground || Thread.CurrentThread.IsThreadPoolThread)
            {
                AutoResetEvent autoEvent = new AutoResetEvent(false);
                //await searchFolder.LoadChildrenAsync();
                //Parent.ExecuteRefreshCommand(new CommandArgs(null, "Search", searchFolder.FullPath));
                //SearchCallback(searchFolder, Items, regPattern);
                //autoEvent.Set();
#if DEBUG
                Plugin.Logger.Debug("->^^^^^^ Current searching folder:{0}", searchFolder.FullPath);
#endif
                autoEvent.WaitOne();
            }
            else
            {
                //var folder = await searchFolder.LoadChildrenAsync();
                //SearchCallback(searchFolder, folder.Files, regPattern);
                //Parent.ExecuteRefreshCommand(new CommandArgs(null, "Search", searchFolder.FullPath));
            }
        }

        private void SearchCallback(IFolder searchFolder, IEnumerable<IFile> subItems, string regPattern)
        {
            if (subItems.IsNullOrEmpty() || this.IsCanceled)
            {
                if (!searchFolder.IsLoading)
                {
                    lock (lockObj)
                    {
                        searchStateList.Remove(searchFolder);
                        if (searchStateList.IsNullOrEmpty())
                        {
                            this.IsSearching = false;
                        }
                    }
                }
                return;
            }

            IEnumerable<IFile> tempList = subItems.Where(item => SearchIsMatch(item, this.regPattern));
            try
            {
                ///list cleared by other thread
                foreach (var item in subItems.Where(item => item.IsFolder))
                {
                    IFolder folder = item as IFolder;
                    SearchFolder(folder);
                }
            }
            catch (InvalidOperationException)
            {
            }

            if (!tempList.IsNullOrEmpty() && !this.IsCanceled)
            {
                this.AddItemsByChunk(tempList, this.Items);
            }

            if (!searchFolder.IsLoading)
            {
                lock (lockObj)
                {
                    searchStateList.Remove(searchFolder);
                    if (searchStateList.IsNullOrEmpty())
                    {
                        this.IsSearching = false;
                    }
                }
            }
        }

        protected const int chunk = 50;
        protected bool AddItemsByChunk<T>(IEnumerable<T> source, IList destination) where T : IFile
        {
            if (source.IsNullOrEmpty() || destination.IsNull())
            {
                return true;
            }

            int index = 0;
            int getCount = chunk;
            while (getCount > 0)
            {
                var chunkItems = source.Skip(index++ * chunk).Take(chunk);
                getCount = chunkItems.Count();
                if (IsCanceled)
                {
                    return false;
                }

                if (getCount == 0)
                {
                    break;
                }

                RunOnUIThread(() =>
                {
                    foreach (var item in chunkItems)
                    {
                        if (IsCanceled)
                        {
                            return;
                        }

                        IFile file = item.Clone() as IFile;
                        if (!file.IsNull())
                        {
                            if (this.IsChecked == true || this.IsChecked == false)
                            {
                                file.IsChecked = this.IsChecked;
                            }
                            destination.Add(file);
                            //CheckMediaType(file);
                        }
                    }
                    SetSearchHint();
                });
            }
            return true;
        }

        private bool SearchIsMatch(IFile file, string pattern)
        {
            bool result = false;
            if (file.IsNull() || file.Name.IsNullOrEmpty())
            {
                return result;
            }
            result = pattern.IsNullOrEmpty() ||
                     Regex.IsMatch(file.Name, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return result;
        }

        private string GetRegexPattern(string keyword)
        {
            string pattern = keyword;
            if (pattern.IsNullOrEmpty())
            {
                return pattern;
            }

            string[] reserverChars = { @"\", "/", "^", "$", "[", "]", "{", "}", ".", "+" };
            foreach (var item in reserverChars)
            {
                if (pattern.Contains(item))
                {
                    pattern = pattern.Replace(item, @"\" + item);
                }
            }

            IDictionary<string, string> wildCharsDic = new Dictionary<string, string>() { { "*", ".*" }, { "?", ".?" } };
            const string regStartPattern = "^";
            const string regEndPattern = "$";
            foreach (var key in wildCharsDic.Keys)
            {
                if (pattern.StartsWith(key))
                {
                    pattern = string.Format("{0}{1}", pattern.Replace(key, wildCharsDic[key]), regEndPattern);
                }
                else if (pattern.EndsWith(key))
                {
                    pattern = string.Format("{0}{1}", regStartPattern, pattern.Replace(key, wildCharsDic[key]));
                }
                else if (pattern.Contains(key))
                {
                    pattern = pattern.Replace(key, wildCharsDic[key]);
                }
            }

            return pattern;
        }

        #endregion
    }
}