using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure.Enums;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Core;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;
using Size = System.Drawing.Size;

namespace Intellidesk.AcadNet.Views
{
    public class CableEnumValueConverter : EnumValueConverter<eCableType> { }

    public partial class CablePanelView : IPanelTabView
    {
        public Common.Enums.PaletteNames PanelTabName => Common.Enums.PaletteNames.Cable;
        //private EditorEvents editorEvents;
        public ICablePanelContext PanelDataContext { get; }

        #region <ctor>

        public CablePanelView(Func<ICablePanelContext> getDataContext)
        {
            //AutoCompleteTextBox uc = new AutoCompleteTextBox();
            //AutoCompleteTextBoxWrapPanel.Children.Add(uc);
            DataContext = PanelDataContext = getDataContext();

            try
            {
                InitializeComponent();
            }
            catch (Exception)
            {
                // ignored
            }

            Loaded += (obj, e) =>
            {
                ((IBasePanelContext)DataContext)?.Load?.Invoke(this);
                //CablePanelDataContext.LoadEvents();
            };
        }

        #endregion

        #region <Property>

        public Size MinimumSize { get; set; }
        public Size MaximumSize { get; set; }
        public PaletteViewStatus Status { get; set; }
        public Autodesk.AutoCAD.Windows.StateEventIndex TabState { get; set; }
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

        #region <events and methods>

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item =
                ItemsControl.ContainerFromElement(sender as ListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item != null)
            {
                var objectId = ((ObjectIdItem)item.Content).ObjectId;
                if (objectId != ObjectId.Null)
                {
                    ((SearchTextPanelContext)DataContext).CommandLine.Zoom(((ObjectIdItem)item.Content).ObjectId);
                    //((SearchTextViewModel)DataContext).ExistListItems.ForEach(x => x.ObjectId.XCast<DBText>().Unhighlight());
                    //var ent = objectId.XCast<DBText>();
                    //ent.Highlight();
                }
            }
        }

        #endregion

        #region <Methods>

        public void Apply()
        {
            throw new NotImplementedException();
        }

        public void Refresh(bool flagManualChange = false)
        {
            this.ExistTextList.ItemsSource = PanelDataContext.ElementItems;
        }

        public void OnActivate(ICommandArgs argument = null)
        {
            if (argument != null)
                PanelDataContext.EditCommand.Execute(argument.CommandParameter);

            PanelDataContext.RegisterEvents();
        }

        public void OnDeactivate()
        {
            PanelDataContext.UnregisterEvents();
        }

        #endregion
    }
}
