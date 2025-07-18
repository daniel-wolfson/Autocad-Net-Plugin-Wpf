using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Size = System.Drawing.Size;

namespace Intellidesk.AcadNet.Views
{
    //public class FilterLayerEnumValueConverter : EnumValueConverter<eCableType> { }
    public partial class QueryBaysPanelView : IPanelTabView
    {
        public PaletteNames PanelTabName => PaletteNames.BayQueries;
        public IBayQueriesPanelContext PanelDataContext { get; }

        #region <ctor>

        public QueryBaysPanelView(Func<IBayQueriesPanelContext> getDataContext)
        {
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
            };
        }

        #endregion

        #region <Commands>

        private ICommand _refreshCommand;
        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand<object>(ExecuteRefresh));

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

        #region <Events and methods>

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

        public void ExecuteRefresh(object value)
        {
            var command = value as CommandArgs ?? new CommandArgs(value, "Refresh", "FullPath");
            ExecuteCommand(command);
        }

        public ICommandArgs ExecuteCommand(ICommandArgs command)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Notifications.DisplayNotifyMessage(NotifyStatus.Working);
            bool isCursorDelay = false;

            var commandName = command.CommandName.ToUpper();
            switch (commandName)
            {
                case "REFRESH":
                    break;

            }
            if (!command.CancelToken.IsCancellationRequested && command.CommandParameter != null)
            {
            }

            if (isCursorDelay)
                InfraManager.DelayAction(1000, (Action)(() =>
                {
                    Notifications.SendNotifyMessageAsync((ID.Infrastructure.Models.NotifyArgs)command.NotifyArgs);
                    Mouse.OverrideCursor = null;
                }));
            else
            {
                Notifications.SendNotifyMessageAsync(command.NotifyArgs);
                Mouse.OverrideCursor = null;
            }

            command.Clean();
            return command;
        }

        public void Refresh(bool flagManualChange = false)
        {
            //this.ExistTextList.ItemsSource = PanelDataContext.ElementItems;
        }

        public void OnActivate(ICommandArgs argument = null)
        {
            //if (argument != null)
            //    PanelDataContext.EditCommand.Execute(argument.CommandParameter);

            //PanelDataContext.LoadEvents();
        }

        public void OnDeactivate()
        {
            //PanelDataContext.RemoveEvents();
        }

        #endregion
    }
}
