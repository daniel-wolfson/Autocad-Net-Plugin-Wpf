using Autodesk.Windows;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class RibbonControlExtensions
    {
        public static bool ContainsTab(this RibbonControl control, string ribbonTabName)
        {
            return control != null && control.Tabs.Any(tab => tab.Id.Equals(ribbonTabName) && tab.Title.Equals(ribbonTabName));
        }

        /// <summary>Find Tab </summary>
        public static RibbonTab FindTab(this RibbonControl control, string ribbonTabName)
        {
            return control.Tabs.FirstOrDefault(tab => tab.Id.Equals(ribbonTabName) && tab.Title.Equals(ribbonTabName));
        }

        /// <summary> Is Ribbon Current </summary>
        public static bool IsTabCurrent(this RibbonControl control, string ribbonTabName)
        {
            return control.Tabs.Any(tab => tab.Id.Equals(ribbonTabName) && tab.Title.Equals(ribbonTabName) && tab.IsActive);
        }

        //public static void DisplayNotifyMessage(this RibbonControl ribbonControl, string notifyMessage)
        //{
        //    Notifications.DisplayNotifyMessage(new NotifyArgs(NotifyStatus.Info, notifyMessage));
        //}

        //public static void DisplayNotifyMessageAsync(this RibbonControl ribbonControl, string notifyMessage)
        //{
        //    Notifications.DisplayNotifyMessageAsync(new NotifyArgs(NotifyStatus.Info, notifyMessage));
        //}

        //public static void DisplayNotifyMessage(this RibbonControl ribbonControl, NotifyArgs args)
        //{
        //    Notifications.DisplayNotifyMessage(args);
        //}

        //public static void SendNotifyMessage(this RibbonControl ribbonControl, string notifyMessage)
        //{
        //    IEventAggregator eventAggregator = Plugin.GetService<IEventAggregator>();
        //    eventAggregator.GetEvent<NotifyMessageStringEvent>().Publish(notifyMessage);
        //}
    }
}