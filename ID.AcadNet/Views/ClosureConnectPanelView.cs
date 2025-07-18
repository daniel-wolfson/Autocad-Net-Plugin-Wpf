using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Core;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using Intellidesk.Data.Models.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;
using Size = System.Drawing.Size;

namespace Intellidesk.AcadNet.Views
{
    public partial class ClosureConnectPanelView : IPanelTabView
    {
        public class ClosureEnumValueConverter : EnumValueConverter<eOpenCloseType> { }

        #region <ctor>
        public ClosureConnectPanelView(Func<IClosureConnectPanelContext> getDataContext)
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
                //ConnectorPanelDataContext.LoadEvents();
            };
        }
        #endregion

        #region <properties>

        public Common.Enums.PaletteNames PanelTabName => Common.Enums.PaletteNames.ClosureConnect;
        public IClosureConnectPanelContext PanelDataContext { get; }
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

        #endregion <properties>

        #region <events>

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
                    ((SearchTextPanelContext)DataContext).ExistListItems.ForEach(x => x.ObjectId.XCast<DBText>().Unhighlight());
                    var ent = objectId.XCast<DBText>();
                    ent.Highlight();
                }
            }
        }

        #endregion <events>

        #region <methods>

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
            if (argument == null) return;

            PanelDataContext.UnregisterEvents();
            switch (argument.CommandName)
            {
                case CommandNames.XLayersLoadData:
                default:
                    PanelDataContext.EditCommand.Execute(argument.CommandParameter);
                    break;
            }
            PanelDataContext.RegisterEvents();
        }

        public void OnDeactivate()
        {
            PanelDataContext.UnregisterEvents();
        }

        private void ConnectorTypesCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is IPaletteElement)
            {
                var seletedItem = e.AddedItems[0] as IPaletteElement;
                var colorIndex = (short)seletedItem.ColorIndex;
                var bodyElementDataContext = PanelDataContext.BodyElementDataContext;

                bodyElementDataContext.ColorDataContext.CurrentColor =
                    bodyElementDataContext.ColorDataContext.Colors.FirstOrDefault(x => x.ColorIndex == colorIndex);

                bodyElementDataContext.CurrentElement.TypeCode = seletedItem.TypeCode;
                bodyElementDataContext.CurrentElement.ColorIndex = colorIndex;
            }
        }

        #endregion
    }
}
