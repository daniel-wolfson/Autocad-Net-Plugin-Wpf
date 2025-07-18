using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.General;
using ID.Infrastructure.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using acApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.WebBrowser
{
    public class WebBrowserInitializer : IExtensionApplication
    {
        public static System.Windows.Controls.WebBrowser WebBrowser;
        public static string Url;

        public static bool IsHostExistAlert()
        {
            if (!ProcessManager.IsProcessRunning("ID.SignalRSelfHost"))
            {
                acApp.ShowAlertDialog("Host process not found! Need execute file: ID.SignalRSelfHost.exe");
                AcadNetHttpServerHost.SignalRClientHost?.LoadHostAsync();
                return false;
            }
            return true;
        }

        public async void Initialize()
        {
            if (WebBrowser != null)
            {
                WebBrowser.Navigating -= browser_Navigating;
                WebBrowser.Navigated -= browser_Navigated;
                WebBrowser = null;
            }

            WebBrowser = new System.Windows.Controls.WebBrowser
            {
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            WebBrowser.Navigating += browser_Navigating;
            WebBrowser.Navigated += browser_Navigated;

            bool initResult = await AcadNetManager.HttpServerHostInitializer.InitializeSignalRAsync();
            if (initResult)
            {
                AcadNetSignalRClientHost signalRClientHost = AcadNetHttpServerHost.SignalRClientHost;
                if (!signalRClientHost.IsConnected)
                    await signalRClientHost.LoadHostAsync();

                IPluginSettings pluginSetting = Plugin.GetService<IPluginSettings>();
                Uri url = string.IsNullOrEmpty(Url)
                    ? new Uri($"{pluginSetting.MapitWebHost}{pluginSetting.MapItPath}")
                    : new Uri(Url);
                WebBrowser.Navigate(url);
            }
        }

        public void Terminate()
        {
            // Make sure we shutdown the core last.
            if (WebBrowser != null && WebBrowser.IsInitialized)
                WebBrowser.Dispose();
        }

        void browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        void browser_Navigated(object sender, NavigationEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }
    }
}