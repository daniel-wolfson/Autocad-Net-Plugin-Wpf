using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Intellidesk.AcadNet
{
    public class WebBrowserManager
    {
        private WebBrowser web_browser;
        private string _url;

        public WebBrowserManager()
        {
            web_browser = new WebBrowser();
        }

        private void DockableDialogs_Loaded(object sender, RoutedEventArgs e)
        {
            web_browser.Navigated += WebBrowser_Navigated;
            web_browser.Navigate(_url);
        }

        void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            HideJsScriptErrors((WebBrowser) sender);
        }

        public void HideJsScriptErrors(WebBrowser wb)
        {
            // IWebBrowser2 interface
            // Exposes methods that are implemented by the 
            // WebBrowser control
            // Searches for the specified field, using the 
            // specified binding constraints.

            FieldInfo fld = typeof (WebBrowser).GetField("_axIWebBrowser2",
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
