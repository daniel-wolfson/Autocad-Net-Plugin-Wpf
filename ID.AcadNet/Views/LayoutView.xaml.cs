using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Common;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Components;
using Intellidesk.AcadNet.ViewModels;
using Intellidesk.Infrastructure;

using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;
using PaletteViewStatus = Intellidesk.AcadNet.Common.Enums.PaletteViewStatus;
using Size = System.Drawing.Size;
using StateEventIndex = Intellidesk.AcadNet.Common.Interfaces.StateEventIndex;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Plugin = Intellidesk.Infrastructure.Plugin;

namespace Intellidesk.AcadNet.Views
{
    /// <summary> Layout View </summary>
    public partial class LayoutView: IPanelTabView
    {
        protected IPluginSettings _pluginSettingss;
        public static CustomTreeView _treeExplorer;

                public MapViewModel ViewModel => (MapViewModel)DataContext
                       ?? Plugin.GetService<IMapViewModel>() as MapViewModel;

        //public Uri Source
        //{
        //    get { return (Uri)GetValue(SourceProperty); }
        //    set { SetValue(SourceProperty, value); }
        //}

        //// Identifies the <see cref="Source"/> dependency property.
        //public static readonly DependencyProperty SourceProperty =
        //    DependencyProperty.Register("Source", typeof(Uri), typeof(MapView), new FrameworkPropertyMetadata(null));


        // This will be set to the created child view that the WebControl will wrap,
        // when ShowCreatedWebView is the result of 'window.open'. The WebControl, 
        // is bound to this property.
        //public IntPtr NativeView
        //{
        //    get { return (IntPtr)GetValue(NativeViewProperty); }
        //    private set { this.SetValue(MapView.NativeViewPropertyKey, value); }
        //}

        //private static readonly DependencyPropertyKey NativeViewPropertyKey =
        //    DependencyProperty.RegisterReadOnly("NativeView",
        //    typeof(IntPtr), typeof(MapView), new FrameworkPropertyMetadata(IntPtr.Zero));

        //// Identifies the <see cref="NativeView"/> dependency property.
        //public static readonly DependencyProperty NativeViewProperty =
        //    NativeViewPropertyKey.DependencyProperty;

        //// The visibility of the address bar and status bar, depends
        //// on the value of this property. We set this to false when
        //// the window wraps a WebControl that is the result of JavaScript
        //// 'window.open'.
        //public bool IsRegularWindow
        //{
        //    get { return (bool)GetValue(IsRegularWindowProperty); }
        //    private set { this.SetValue(MapView.IsRegularWindowPropertyKey, value); }
        //}

        //private static readonly DependencyPropertyKey IsRegularWindowPropertyKey =
        //    DependencyProperty.RegisterReadOnly("IsRegularWindow",
        //    typeof(bool), typeof(MapView), new FrameworkPropertyMetadata(true));

        //// Identifies the <see cref="IsRegularWindow"/> dependency property.
        //public static readonly DependencyProperty IsRegularWindowProperty =
        //    IsRegularWindowPropertyKey.DependencyProperty;

        /// <summary> ctor </summary>
        public LayoutView()
        {
            //WebBrowser.Navigated += WebBrowser_Navigated;
            //System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

            //// Create the ActiveX control.
            //WebKit.WebKitBrowser browser = new WebKit.WebKitBrowser();
            //browser.Navigate("http://www.google.com");
            //// Assign the ActiveX control as the host control's child.
            //host.Child = browser;

            //// Add the interop host control to the Grid
            //// control's collection of child controls.
            //this.Grid1.Children.Add(host);

            //WebKit.WebKitBrowser browser = new WebKit.WebKitBrowser();


            // Assign the ActiveX control as the host control's child.
            //WndHost.Child = browser;

            // Add the interop host control to the Grid 
            // control's collection of child controls. 

            //grdBrowserHost.Children.Add(host);

            //System.Windows.Forms.TabControl tabcon = new System.Windows.Forms.TabControl();
            //TabPage NewTab = new TabPage();
            //Tabkit NewBrowser = new Tabkit();
            //NewTab.Tag = NewBrowser;
            //NewBrowser.Tag = NewTab;
            //tabcon.TabPages.Add(NewTab);
            //NewTab.Controls.Add(NewBrowser);
            //NewBrowser.Dock = DockStyle.Fill;
            //NewBrowser.Navigate("http://www.google.com");

            //WndHost.Child.Controls.Add(tabcon);

            //Name = "MapIt";
            //Title = "Map Explorer";
            //_unityContainer = Plugin.Container;
            //_appSettings = _unityContainer.Resolve<IPluginSettings>();
            //DataContext = _unityContainer.Resolve<IMapViewModel>();

            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                App.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
            }


            //Map.Navigate(new Uri(@"file:///C:\Projects\IntelliDesk\ID.Map\wwwroot\MapIt.html"));
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


        public void OnActivate(CommandArgs argument = null)
        {
            //var pluginSettings = Plugin.GetService<IPluginSettings>();
            var url = "https://www.google.ru/maps"; //"@"file:///" https://www.google.ru/maps" pluginSettings.MapItPath
            //WebBrowser.Navigate(_url);
            webBrowser.Source = new Uri(url);
            webBrowser.GoToHome();
            //Application.DocumentManager.DocumentActivated -= CablePanelDataContext.OnDocumentActivated;
            //Application.DocumentManager.DocumentActivated += CablePanelDataContext.OnDocumentActivated;
        }

        public void OnDeactivate()
        {
            //Application.DocumentManager.DocumentActivated -= CablePanelDataContext.OnDocumentActivated;
        }

        public void Refresh(bool flagManualChange = false)
        {
            //throw new NotImplementedException();
        }

        public void Apply()
        {
            //throw new NotImplementedException();
        }

        private void ExpanderExpanded(object sender, RoutedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void ExpanderCollapsed(object sender, RoutedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        #region Implementation ITabView

        public Size MinimumSize { get; set; }
        public Size MaximumSize { get; set; }
        public PaletteViewStatus Status { get; set; }
        public StateEventIndex TabState { get; set; }
        public Palette ParentPalette { get; set; }
        public object ParentPaletteSet { get; set; }
        public bool IsActive { get; set; }
        public bool Current { get; set; }
        public string Title { get; set; }
        public int UniId { get; set; }
        public bool Visible { get; set; }
        public bool Complete { get; set; }
        public string Comment { get; set; }
        public Size Size { get; set; }
        public CommandArgs ActivateArgument { get; set; }

        #endregion
    }

    //public class Tabkit : WebKit.WebKitBrowser
    //{
    //    //public Tabkit()
    //    //{
    //    //    NewWindowRequest += Tabkit_NewWindowRequest;
    //    //    Navigating += Tabkit_Navigating;
    //    //    Navigated += Tabkit_Navigated;
    //    //    Error += Tabkit_Error;
    //    //    DocumentCompleted += Tabkit_DocumentCompleted;
    //    //}

    //    private void Tabkit_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    //    {
    //        //Form1.pnl_err.Visible = false;
    //        //Form1.Text = "TabwebKit Browser - " + this.DocumentTitle;
    //        //TabPage NewTabPage = this.Tag;
    //        //NewTabPage.Text = this.DocumentTitle;
    //        //Form1.txtbx_url.Text = this.Url.AbsoluteUri.ToString;
    //        //Form1.lbl_WebsiteName.Text = this.DocumentTitle;
    //        if (this.CanGoBack == true)
    //        {
    //            //Form1.btn_Back.Enabled = true;
    //        }
    //        else
    //        {
    //            //Form1.btn_Back.Enabled = false;
    //        }
    //        if (this.CanGoForward == true)
    //        {
    //            //Form1.btn_Forward.Enabled = true;
    //        }
    //        else
    //        {
    //            //Form1.btn_Forward.Enabled = false;
    //        }
    //    }

    //    //private void Tabkit_Error(object sender, WebKitBrowserErrorEventArgs e)
    //    //{
    //    //    //Form1.pnl_err.Visible = true;
    //    //    //Form1.lbl_StatusText.Text = "Error!";
    //    //}

    //    private void Tabkit_Navigated(object sender, WebBrowserNavigatedEventArgs e)
    //    {
    //        //Form1.lbl_StatusText.Text = "Done";
    //        //Form1.pnl_status.Visible = false;
    //    }

    //    private void Tabkit_Navigating(object sender, WebBrowserNavigatingEventArgs e)
    //    {
    //        //Form1.pnl_status.Visible = true;
    //        //Form1.lbl_StatusText.Text = "Loading...";
    //    }


    //    private void Tabkit_NewWindowRequest(object sender, NewWindowRequestEventArgs e)
    //    {
    //    }
    //}


}
