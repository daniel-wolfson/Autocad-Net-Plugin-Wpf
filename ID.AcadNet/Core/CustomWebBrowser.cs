using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace Intellidesk.AcadNet.Core
{
    public class WebBrowserManager
    {
        private readonly System.Windows.Controls.WebBrowser web_browser;
        private readonly string _url = "";

        public WebBrowserManager()
        {
            web_browser = new System.Windows.Controls.WebBrowser();
        }

        private void DockableDialogs_Loaded(object sender, RoutedEventArgs e)
        {
            web_browser.Navigated += WebBrowser_Navigated;
            web_browser.Navigate(_url);
        }

        void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            HideJsScriptErrors((System.Windows.Controls.WebBrowser) sender);
        }

        public void HideJsScriptErrors(System.Windows.Controls.WebBrowser wb)
        {
            // IWebBrowser2 interface
            // Exposes methods that are implemented by the 
            // WebBrowser control
            // Searches for the specified field, using the 
            // specified binding constraints.

            FieldInfo fld = typeof (System.Windows.Controls.WebBrowser).GetField("_axIWebBrowser2",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (null != fld)
            {
                object obj = fld.GetValue(wb);
                if (null != obj)
                {
                    // Silent: Sets or gets a value that indicates 
                    // whether the object can display dialog boxes.
                    // HRESULT IWebBrowser2::get_Silent(VARIANT_BOOL *pbSilent);
                    // HRESULT IWebBrowser2::put_Silent(VARIANT_BOOL bSilent);

                    obj.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, obj, new object[] {true});
                }
            }
        }
    }
}
