using FileExplorer.Model;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.ViewModel
{
    public abstract class SortOrderViewModel : ViewModelBase, ISortOrder
    {
        ICollectionView _contentView;
        GridViewColumn _lastColumn;

        public ResourceDictionary ResourceDictionary;


        public ICollectionView FolderDetailsCollectionView
        {
            get { return _contentView; }
            protected set { SetProperty(ref _contentView, value, "FolderDetailsCollectionView"); }
        }

        #region Sort order

        private SortOrderCallback _sortOrderCallback = null;

        protected SortOrderViewModel()
        {
            ResourceDictionary = new ResourceDictionary()
            {
                Source = new Uri("/ID.AcadNet;component/Assets\\Styles.xaml", UriKind.Relative)
            };
        }

        public void SetSortOrderCallback(SortOrderCallback callback)
        {
            if (callback.IsNull())
            {
                return;
            }
            _sortOrderCallback = callback;
        }

        protected void InvokeSortOrderCallback(bool isRefresh = true)
        {
            if (!_sortOrderCallback.IsNull())
            {
                _sortOrderCallback(isRefresh);
            }
        }

        public const string SortPropertyName = "Name";
        public const string SortPropertyIsFolder = "IsFolder";
        public const string SortPropertyIsFile = "IsFile";

        public void SetSortOrder(GridViewColumn column, ListSortDirection? sortDirection = null)
        {
            if (column == null) return;

            var header = (GridViewColumnHeader)column.Header;
            header.Name = column.GetSortPropertyValue();

            if (sortDirection == null)
            {
                SortDescription sort =
                    FolderDetailsCollectionView.SortDescriptions.FirstOrDefault(x => x.PropertyName == header.Name);
                sortDirection = sort.Direction == ListSortDirection.Ascending
                    ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }

            if (_lastColumn != null && ((GridViewColumnHeader)_lastColumn.Header).Name != header.Name)
                ((GridViewColumnHeader)_lastColumn.Header).ContentTemplate =
                    ResourceDictionary["NormalSortHeaderTemplate"] as DataTemplate;

            _lastColumn = column;

            header.ContentTemplate = (sortDirection == ListSortDirection.Ascending)
                ? ResourceDictionary["AscSortHeaderTemplate"] as DataTemplate
                : ResourceDictionary["DescSortHeaderTemplate"] as DataTemplate;

            SetSortOrder(header.Name, (ListSortDirection)sortDirection);
        }

        public void SetSortOrder(string propName, ListSortDirection sortDirection)
        {
            if (propName.IsNullOrEmpty() || this.FolderDetailsCollectionView.IsNull()) return;

            this.ClearSortOptions();
            try
            {
                if (sortDirection == ListSortDirection.Ascending)
                {
                    this.FolderDetailsCollectionView.SortDescriptions.Add(new SortDescription(SortPropertyIsFolder, ListSortDirection.Descending));
                    this.FolderDetailsCollectionView.SortDescriptions.Add(new SortDescription(propName, sortDirection));
                }
                else
                {
                    this.FolderDetailsCollectionView.SortDescriptions.Add(new SortDescription(SortPropertyIsFile, ListSortDirection.Descending));
                    this.FolderDetailsCollectionView.SortDescriptions.Add(new SortDescription(propName, sortDirection));
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected void ClearSortOptions()
        {
            if (!this.FolderDetailsCollectionView.IsNull() && this.FolderDetailsCollectionView.SortDescriptions != null)
            {
                this.FolderDetailsCollectionView.SortDescriptions.Clear();
            }
        }

        #endregion

        protected override void OnDisposing(bool isDisposing)
        {
            this._sortOrderCallback = null;
            this.ClearSortOptions();
            base.OnDisposing(isDisposing);
        }
    }
}
