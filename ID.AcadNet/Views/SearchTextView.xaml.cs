using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;
using Size = System.Drawing.Size;

namespace Intellidesk.AcadNet.Views
{
    public partial class SearchTextView : IPanelTabView
    {
        #region ctor
        public SearchTextView()
        {
            //AutoCompleteTextBox uc = new AutoCompleteTextBox();
            //AutoCompleteTextBoxWrapPanel.Children.Add(uc);

            Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            try
            {
                InitializeComponent();
            }
            catch (Exception)
            {
                // ignored
            }

            DataContext = Plugin.GetService<ISearchTextPanelContext>() as SearchTextPanelContext;

            //SearchDataContext = DataContext as SearchTextViewModel;
            //Loaded += (obj, e) =>
            //{
            //    if (((IPanel)DataContext).Load != null)
            //        ((IPanel)DataContext).Load(this);
            //};
        }
        #endregion

        #region <props>

        public PaletteNames PanelTabName => PaletteNames.Search;
        public SearchTextPanelContext SearchDataContext => (SearchTextPanelContext)DataContext;
        //public SearchTextViewModel SearchDataContext { get; set; } // => (SearchTextViewModel)DataContext

        public Size MinimumSize { get; set; }
        public Size MaximumSize { get; set; }
        public PaletteViewStatus Status { get; set; }
        public StateEventIndex TabState { get; set; }
        public Palette ParentPalette { get; set; }
        public object ParentPaletteSet { get; set; }
        public bool IsLive { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public int UniId { get; set; }
        public bool Visible { get; set; }
        public bool Complete { get; set; }
        public string Comment { get; set; }
        public Size Size { get; set; }
        public ICommandArgs ActivateArgument { get; set; }

        #endregion

        #region <methods>

        public void OnActivate(ICommandArgs argument = null)
        {
            acadApp.DocumentManager.DocumentActivated -= SearchDataContext.OnDocumentActivated;

            if (argument == null) return;

            switch (argument.CommandName)
            {
                case CommandNames.XLayersLoadData:
                    SearchDataContext.RefreshCommand.Execute(argument);
                    break;
                default:
                    if (argument.CommandParameter != null && argument.CommandParameter.ToString() != "")
                        SearchDataContext.RunCommand.Execute(argument.CommandParameter);
                    break;
            }

            acadApp.DocumentManager.DocumentActivated += SearchDataContext.OnDocumentActivated;
        }

        public void OnDeactivate()
        {
            acadApp.DocumentManager.DocumentActivated -= SearchDataContext.OnDocumentActivated;
        }

        public void Apply()
        {
            throw new NotImplementedException();
        }

        public void Refresh(bool flagManualChange = false)
        {
            this.ExistTextList.ItemsSource = SearchDataContext.ExistListItems;
        }

        #endregion

        #region <callbacks>

        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            e.Handled = true;
        }

        private void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            acadApp.DocumentManager.DocumentToBeDestroyed -= SearchDataContext.OnDocumentToBeDestroyed;
            acadApp.DocumentManager.DocumentActivated -= SearchDataContext.OnDocumentActivated;
        }

        private void SelectedIndex_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            ObjectIdItem objectIdItem = item?.Content as ObjectIdItem;
            if (objectIdItem != null)
            {
                var objectId = objectIdItem.ObjectId;
                if (objectId != ObjectId.Null)
                {
                    //((SearchTextViewModel)DataContext).CommandLine.Zoom(((ObjectIdItem)item.Content).ObjectId);
                    ((SearchTextPanelContext)DataContext).OnSelectedItem(objectIdItem);
                    //((SearchTextViewModel)DataContext).ExistListItems.ForEach(x => x.ObjectId.XCast<DBText>().Unhighlight());
                    //var ent = objectId.XCast<DBText>();
                    //ent.Highlight();
                }
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length == 0)
                ((TextBox)sender).Text = ((TextBox)sender).Tag.ToString();
        }

        private void TextPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (((TextBox)sender).Text == ((TextBox)sender).Tag.ToString())
            {
                ((TextBox)sender).Text = e.Text;
                e.Handled = true;
                ((TextBox)sender).SelectionStart = 1;
            }
        }

        #endregion <callbacks>

    }
}
