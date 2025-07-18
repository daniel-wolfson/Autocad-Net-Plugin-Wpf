using Autodesk.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Extensions;
using Prism.Events;
using System.Linq;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Core
{
    public static class Notifications
    {
        private static IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();
        private static IEventAggregator EventAggregator => Plugin.GetService<IEventAggregator>();
        private static RibbonControl Rc => ComponentManager.Ribbon;

        #region <DisplayNotifyMessage>

        public static void DisplayNotifyMessage(ITaskArguments args)
        {
            NotifyStatus notifyImageStatus = args.CommandInfo.Keys.Any(x => x == NotifyStatus.Error)
                ? NotifyStatus.Error
                : NotifyStatus.Ready;

            SendNotifyMessageAsync(new NotifyArgs(notifyImageStatus, args.DisplayName));
        }

        public static void DisplayNotifyMessage(string notifyMessage)
        {
            WriteMessage(notifyMessage);
        }

        public static void DisplayNotifyMessage(NotifyStatus status)
        {
            Send(new NotifyArgs(status));
        }
        public static void DisplayNotifyMessageAsync(string notifyMessage)
        {
            WriteMessage(notifyMessage);
        }
        public static void DisplayNotifyMessageAsync(string notifyMessage, int delay)
        {
            Plugin.DelayAction(delay, () => WriteMessage(notifyMessage));
        }
        private static void WriteMessage(string notifyMessage)
        {
            if (Rc != null && Rc.IsVisible && Rc.ContainsTab(PluginSettings.Name))
                Send(new NotifyArgs(NotifyStatus.Working, notifyMessage));

            if (App.DocumentManager.MdiActiveDocument != null)
            {
                var ed = App.DocumentManager.MdiActiveDocument.Editor;
                notifyMessage += notifyMessage.ToLower().Contains("working")
                    || notifyMessage.ToLower().Contains("loading")
                    ? "..." : "";

                ed.WriteMessage(PluginSettings.Prompt + "plugin " + notifyMessage.ToLower() + "\n");
            }
        }

        #endregion <DisplayNotifyMessage>

        #region <SendNotifyMessage>

        public static void SendNotifyMessageAsync(NotifyStatus status)
        {
            SendNotifyMessageAsync(new NotifyArgs(status));
        }
        public static void SendNotifyMessageAsync(NotifyArgs args)
        {
            Plugin.DelayAction(200, () => Send(args));
        }
        public static void SendNotifyMessageAsync(NotifyArgs args, int delay)
        {
            Plugin.DelayAction(delay, () => Send(args));
        }
        public static void SendNotifyMessageAsync(NotifyStatus status, int delay)
        {
            Plugin.DelayAction(delay, () => Send(new NotifyArgs(status)));
        }
        public static void SendNotifyMessage(string notifyMessage)
        {
            EventAggregator.GetEvent<NotifyMessageStringEvent>().Publish(notifyMessage);
        }
        private static void Send(NotifyArgs args)
        {
            EventAggregator.GetEvent<NotifyMessageEvent>().Publish(args);
        }

        #endregion <SendNotifyMessage>
    }
}