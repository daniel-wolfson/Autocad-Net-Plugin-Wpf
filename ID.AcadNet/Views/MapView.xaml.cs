using Autodesk.AutoCAD.Windows;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using Intellidesk.AcadNet.WebBrowser;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using NavigationEventArgs = System.Windows.Navigation.NavigationEventArgs;
using Size = System.Drawing.Size;

namespace Intellidesk.AcadNet.Views
{
    /// <summary> Layout View </summary>
    public partial class MapView : IPanelTabView
    {
        public PaletteNames PanelTabName => PaletteNames.MapView;

        public IMapViewModel MapDataContext => (IMapViewModel)DataContext;

        #region Fields
        private const string CHANGE_IMG_SRC = @"(function() { " +
            "var image = document.getElementById('awe-logo-silhouette'); " +
            "if (image == null) { return; } " +
            "image.addEventListener('load', function() { myObject.onImageLoaded('awe-logo-silhouette', image.src) }, false ); " +
            "image.src='http://upload.wikimedia.org/wikipedia/commons/1/1e/Lascaux_painting.jpg'; })()";
        #endregion

        /// <summary> ctor </summary>
        public MapView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                App.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
            }

            DataContext = Plugin.GetService<IMapViewModel>();

            Loaded += OnLoaded;
            //Unloaded += OnUnloaded;

            //Create the CablePanelDataContext and expose it using the View's DataContext
            //if (dataContext != null)
            //{
            //    DataContext = dataContext;
            //    //if (dataContext.CurrentUserSetting.ProjectExplorerRowSplitterPosition > 0)
            //    MapGrdMain.RowDefinitions[2].Height = new GridLength(
            //        dataContext.CurrentUserSetting.ProjectExplorerRowSplitterPosition, GridUnitType.Pixel);
            //    //this.DataGrid.ItemTemplate = this.ContentTemplateSelector.SelectTemplate(value, this); 
            //}

            //Uri uri = new Uri(@"pack://application:,,,/C:\Projects\IntelliDesk\ID.Map\wwwroot\MapIt.html");
            //Stream source = System.Windows.Application.GetContentStream(uri).Stream;
            //Map.NavigateToStream(source);
            //Map.Navigate(new Uri("http://www.google.com", UriKind.RelativeOrAbsolute));
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            WebBrowserInitializer.WebBrowser.Visibility = Visibility.Collapsed;
            GridMapContainer.Children.Clear();
            WebBrowserInitializer.WebBrowser.Visibility = Visibility.Visible;
            WebBrowserInitializer.WebBrowser.Dispose();
            WebBrowserInitializer.WebBrowser = null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // WebBrowser
            //if (WebBrowserInitializer.WebBrowser == null)
            //{
            //    WebBrowserInitializer webBrowserInitializer = new WebBrowserInitializer();
            //    webBrowserInitializer.Initialize();
            //}
            //InfraManager.DelayAction(200, () =>
            //{
            //    ProjectExplorerDataContext.RefreshCommand.Execute(null);
            //});

            //if (WebBrowserInitializer.WebBrowser != null)
            //    grid.Children.Add(WebBrowserInitializer.WebBrowser);

            //webControl.NativeViewInitialized += OnNativeViewInitialized;
            //webControl.DocumentReady += OnDocumentReady;
            //webControl.ConsoleMessage += OnConsoleMessage;
            //// Start with the specified Home URL.
            //this.Source = WebCore.Configuration.HomeURL;

            // Create a request for the URL. 
            //WebRequest request = WebRequest.Create(_url);
            //// If required by the server, set the credentials.
            //request.Credentials = CredentialCache.DefaultCredentials;
            //// Get the response.
            //WebResponse response = request.GetResponse();
            //// Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            //// Get the stream containing content returned by the server.
            //Stream dataStream = response.GetResponseStream();
            //// Open the stream using a StreamReader for easy access.
            //StreamReader reader = new StreamReader(dataStream);
            //// Read the content.
            //string responseFromServer = reader.ReadToEnd();
            //// Display the content.
            //WebBrowser.NavigateToString(responseFromServer);
            //// Clean up the streams and the response.
            //reader.Close();
            //response.Close();

            // string html;
            // var r = WebRequest.Create(_url);
            // using (var rs = r.GetResponse().GetResponseStream())
            // {
            //     using (var reader = new StreamReader(rs, Encoding.UTF8))
            //     {
            //         html = reader.ReadToEnd();
            //     }
            // }
            // god forbid to do it like this, example only!
            //html = html.Replace("<head>", "<head><base href=\"" + _url + "\"");
            // WebBrowser.NavigateToString(html);

            //Commands.RunOnUIThread(() =>
            //{
            //    //WebBrowser = new WebBrowser();
            //    WebBrowser.Navigating += new NavigatingCancelEventHandler(browser_Navigating);
            //    WebBrowser.Navigated += new NavigatedEventHandler(browser_Navigated);
            //    WebBrowser.Navigate(_url);
            //});

            //var t = Task.Run(() =>

            //WebBrowser.Visibility = System.Windows.Visibility.Hidden;
            //root.Children.Add(webb);

            //!!!!!!!!!!!!!!
            //WebBrowser.LoadCompleted += webb_LoadCompleted;
            //WebBrowser.Navigate(new Uri(_url));

            //{
            //    WebBrowser.Navigating += new NavigatingCancelEventHandler(browser_Navigating);
            //    WebBrowser.Navigated += new NavigatedEventHandler(browser_Navigated);
            //    WebBrowser.Navigate(_url);
            //};
            //t.Wait();

            //var th = new Thread(() =>
            //{
            //    Commands.RunOnUIThread(() =>
            //    {
            //        Grid1.Children.Add(WebBrowserInitializer.WebBrowser);
            //    });
            //});
            //th.SetApartmentState(ApartmentState.STA);
            //th.Start();

            //WebBrowser = new WebBrowser();
            //WebBrowser.Navigating += new NavigatingCancelEventHandler(browser_Navigating);
            //WebBrowser.Navigated += new NavigatedEventHandler(browser_Navigated);
            //WebBrowser.Navigate(new Uri(_url));
        }

        //void wb_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    HideJsScriptErrors((System.Windows.Controls.WebBrowser)sender);
        //    // e.Content
        //}

        #region Implementation IPanelTabView
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

        public void Refresh(bool flagManualChange = false)
        {
            //throw new NotImplementedException();
        }

        public void Apply()
        {
            //throw new NotImplementedException();
        }

        public void OnActivate(ICommandArgs argument = null)
        {
            if (GridMapContainer != null && !GridMapContainer.Children.Contains(WebBrowserInitializer.WebBrowser))
            //argument == null !this.IsActive
            {
                InfraManager.DelayAction(200, () =>
                {
                    MapDataContext.RefreshCommand.Execute(GridMapContainer);
                });
            }
        }

        public void OnDeactivate()
        {
            //Application.DocumentManager.DocumentActivated -= CablePanelDataContext.OnDocumentActivated;
        }
        #endregion

        void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            HideJsScriptErrors((System.Windows.Controls.WebBrowser)sender);
        }

        public void HideJsScriptErrors(System.Windows.Controls.WebBrowser wb)
        {
            // IWebBrowser2 interface
            // Exposes methods that are implemented by the 
            // WebBrowser control
            // Searches for the specified field, using the 
            // specified binding constraints.

            FieldInfo fld = typeof(System.Windows.Controls.WebBrowser).GetField("_axIWebBrowser2",
                BindingFlags.Instance | BindingFlags.NonPublic);

            object obj = fld?.GetValue(wb);
            // Silent: Sets or gets a value that indicates 
            // whether the object can display dialog boxes.
            // HRESULT IWebBrowser2::get_Silent(VARIANT_BOOL *pbSilent);
            // HRESULT IWebBrowser2::put_Silent(VARIANT_BOOL bSilent);

            obj?.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, obj, new object[] { true });
        }

        private void Map_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Notifications.DisplayNotifyMessage("MouseDown");
        }

        private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void ExpanderExpanded(object sender, RoutedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void ExpanderCollapsed(object sender, RoutedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void MapOpenLayoutButton_OnClick(object sender, RoutedEventArgs e)
        {
            //var _url = @"file:///" + _appSettings.MapItPath;
            //WebBrowser.Navigate(new Uri(_url));
        }
    }
}
