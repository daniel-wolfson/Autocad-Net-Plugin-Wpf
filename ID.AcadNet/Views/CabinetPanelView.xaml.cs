using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Core;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;
using Size = System.Drawing.Size;
using Visibility = System.Windows.Visibility;

namespace Intellidesk.AcadNet.Views
{
    public class CabinetEnumValueConverter : EnumValueConverter<eCabinetType> { }
    public partial class CabinetPanelView : IPanelTabView, IBasePanelContext
    {
        public Common.Enums.PaletteNames PanelTabName => Common.Enums.PaletteNames.Cabinet;
        public ICabinetPanelContext PanelDataContext { get; }

        #region <ctor>

        public CabinetPanelView(Func<ICabinetPanelContext> getDataContext)
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
                //CabinetPanelDataContext.LoadEvents();
            };
        }

        #endregion

        #region <Property>

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

        Action<object> IBasePanelContext.Load
        {
            get { return null; }
            set { value = null; }
        }

        #endregion

        #region <events>

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var runButton = Intellidesk.AcadNet.Common.Extensions.DependencyObjectExtensions.FindChild<TextBlock>(this, "NotifyButton");
            if (Validation.GetHasError(TitleText))
            {
                //RunButton.IsEnabled = ProjectExplorerDataContext.SearchViewModel.IsSearchEnabled = false;
                NotifyButton.Visibility = Visibility.Visible;
                NotifyButton.ToolTip = Validation.GetErrors(TitleText)[0].ErrorContent;
            }
            else
            {
                //RunButton.IsEnabled = ProjectExplorerDataContext.SearchViewModel.IsSearchEnabled = true;
                NotifyButton.Visibility = Visibility.Collapsed;
                NotifyButton.ToolTip = "";
            }
        }

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

        #endregion

        #region <Methods>

        public ICommandArgs ExecuteCommand(ICommandArgs command)
        {
            //Mouse.OverrideCursor = Cursors.Wait;
            //Notifications.DisplayNotifyMessage(NotifyStatus.Working);

            switch (command.CommandName)
            {
                case "Search":
                case "Activate":
                case "Load":
                case "ShowAll":
                case "Refresh":
                    break;
            }

            if (!command.CancelToken.IsCancellationRequested && command.CommandParameter != null)
            {
            }

            return command;
        }

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
            //PanelDataContext.RemoveEvents();
        }

        void IBasePanelContext.ExecuteRefreshCommand(object value)
        {
            //throw new NotImplementedException();
        }

        Task<ICommandArgs> IBasePanelContext.ExecuteCommand(ICommandArgs command)
        {
            //throw new NotImplementedException();
            return Task.FromResult<ICommandArgs>(null);
        }
    }

    #endregion
}
