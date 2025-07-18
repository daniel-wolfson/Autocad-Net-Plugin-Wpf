using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using ID.AcadNet.Model;
using ID.Common.Enums;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.AcadNet.ViewModels;
using Intellidesk.Infrastructure;

using ITabView = Intellidesk.AcadNet.Common.Interfaces.ITabView;
using Size = System.Drawing.Size;
using StateEventIndex = Intellidesk.AcadNet.Interfaces.StateEventIndex;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Views
{
    public partial class ClosureView : ITabView
    {
        protected readonly IUnityContainer _unityContainer;
        protected IPluginSettings _appSettings;

        public IClosureElementPanel ElementPanel => (IClosureElementPanel)DataContext
               ?? _unityContainer.Resolve<IClosureElementPanel>();

        #region "ctor"
        public ClosureView()
        {
            _unityContainer = AcadNetManager.Container;
            //AutoCompleteTextBox uc = new AutoCompleteTextBox();
            //AutoCompleteTextBoxWrapPanel.Children.Add(uc);

            try
            {
                InitializeComponent();
            }
            catch (Exception)
            {
                // ignored
            }

            DataContext = _unityContainer.Resolve<IClosureElementPanel>();

            Loaded += (obj, e) =>
            {
                if (((IPanel)DataContext).Load != null)
                    ((IPanel)DataContext).Load(this);
            };
        }
        #endregion

        public void OnActivate(CommandArgs activateArgument = null)
        {
            //Application.DocumentManager.DocumentActivated -= ElementPanel.OnDocumentActivated;
            //Application.DocumentManager.DocumentActivated += ElementPanel.OnDocumentActivated;
            //Application.DocumentManager.DocumentToBeDestroyed -= OnDocumentToBeDestroyed;
            //Application.DocumentManager.DocumentToBeDestroyed += OnDocumentToBeDestroyed;

            if (activateArgument != null)
                ElementPanel.EditCommand.Execute(activateArgument.CommandParameter);

            //ElementPanel.OnDocumentActivated(null, null);
            //Application.DocumentManager.DocumentDestroyed += ElementPanel.ExecuteCloseLayout;
            //Application.DocumentManager.DocumentToBeDeactivated += ElementPanel.OnDocumentToBeDeactivated;
            //Layout.EntityChangedEvent += ElementPanel.OnlayoutPropertyChanged;
        }

        public void OnDeactivate()
        {
            App.DocumentManager.DocumentActivated -= ElementPanel.OnDocumentActivated;
        }

        public void Refresh(bool flagManualChange = false)
        {
            this.ExistTextList.ItemsSource = ElementPanel.ExistListItems;
        }

        #region "Property"

        public Size MinimumSize { get; set; }
        public Size MaximumSize { get; set; }
        public PaletteViewStatus Status { get; set; }
        public StateEventIndex TabState { get; set; }
        public Palette ParentPalette { get; set; }
        public object ParentPaletteSet { get; set; }
        public bool IsActive { get; set; }
        public bool Current { get; set; }
        public string Title { get; set; }
        public int UniId { get; set; }
        public bool Visible { get; set; }
        public bool Complete { get; set; }
        public string Comment { get; set; }
        public Size Size { get; set; }
        public CommandArgs ActivateArgument { get; set; }

        #endregion

        #region "events methods"

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item != null)
            {
                var objectId = ((ObjectIdItem)item.Content).ObjectId;
                if (objectId != ObjectId.Null)
                {
                    ((SearchTextPanel)DataContext).CommandLine.Zoom(((ObjectIdItem)item.Content).ObjectId);
                    ((SearchTextPanel)DataContext).ExistListItems.ForEach(x => x.ObjectId.XCast<DBText>().Unhighlight());
                    var ent = objectId.XCast<DBText>();
                    ent.Highlight();
                }
            }
        }

        private void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            App.DocumentManager.DocumentToBeDestroyed -= ElementPanel.OnDocumentToBeDestroyed;
            App.DocumentManager.DocumentActivated -= ElementPanel.OnDocumentActivated;
        }

        #endregion

        #region "Methods"

        public void Apply()
        {
            throw new NotImplementedException();
        }

        //public void Search(string text)
        //{
        //    ((SearchTextPanel)DataContext).SelectedText = text;
        //    ((SearchTextPanel)DataContext).ExecuteRunCommand("Start");
        //}

        #endregion
    }
}
