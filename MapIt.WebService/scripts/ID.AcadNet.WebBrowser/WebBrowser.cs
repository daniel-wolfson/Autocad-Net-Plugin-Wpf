using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Awesomium.Core;
using Intellidesk.Infrastructure;
using Microsoft.Practices.Unity;
using System.Windows.Media.Imaging;
using Awesomium.Windows.Controls;

namespace ID.AcadNet.WebBrowser
{
    public class WebBrowser
    {
        public static void Initialize()
        {
            IPluginSettings appSettings = PluginBootstrapper.PluginContainer.Resolve<IPluginSettings>();

            // Initialization must be performed here,
            // before creating a WebControl.
            if (!WebCore.IsInitialized)
            {
                WebCore.Initialize(new WebConfig()
                {
                    HomeURL = new Uri(@"file:///" + appSettings.MapItPath), //"awesomium.com".ToUri(),
                    LogPath = @".\starter.log",
                    LogLevel = LogLevel.Verbose,
                    RemoteDebuggingPort = 9033
                });
            }
        }

        public static void Terminate()
        {
            // Make sure we shutdown the core last.
            if (WebCore.IsInitialized)
                WebCore.Shutdown();
        }

        #region Methods
        // We demonstrate getting a screenshot of the view, in WPF.
        private void GetScreenshot(string fileName)
        {
            //WebViewPresenter surface = webControl.Surface as WebViewPresenter;

            //if (surface == null)
            //    return;

            //BitmapSource bitmap = surface.Image as BitmapSource;

            //if (bitmap == null)
            //    return;

            //// For the sample, we use a PNG encoder. WPF provides
            //// other too, such as a JpegBitmapEncoder.
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(bitmap));

            //// Open/Create the file to save the image to.
            //using (FileStream fs = File.Open(fileName, FileMode.OpenOrCreate))
            //    encoder.Save(fs);
        }

        // We demonstrate thread-safe execution against the view.
        private void PrintPageHTML()
        {
            // Code potentially executed in another (non-UI) thread.
            // This code is valid for any web-view component. For WPF only,
            // you can also use webControl.Dispatcher.Invoke.
            ISynchronizeInvoke sync = (ISynchronizeInvoke)webControl;
            string html = sync.InvokeRequired ?
                sync.Invoke((Func<String>)ExecuteJavascriptOnView, null) as String :
                ExecuteJavascriptOnView();

            if (!String.IsNullOrEmpty(html))
            {
                // Managed IWebView instances already provide an HTML property
                // that is updated when the DOM is loaded. At this point,
                // these two should be equal.
                // Debug.Assert( html == webControl.HTML );
                // Print it.
                Debug.Print(html);
            }
        }

        // This method will be called on the WebControl's thread (the UI thread).
        private string ExecuteJavascriptOnView()
        {
            // This method is called from within DocumentReady so it already
            // executes in an asynchronous Javascript Execution Context (JEC).
            var global = Global.Current;

            if (!global)
                return String.Empty;

            // We demonstrate the use of dynamic.
            var document = global.document;

            if (!document)
                return String.Empty;

            // JSObject supports the DLR. You can dynamically call methods,
            // access arrays or lists and get or set properties.
            return document.getElementsByTagName("html")[0].outerHTML;

            // Any binding errors or exceptions that occur in a Javascript Execution 
            // Context (JEC) are propagated to the JavaScript console.
        }
        #endregion

        #region Event Handlers
        private void OnNativeViewInitialized(object sender, WebViewEventArgs e)
        {
            // The native view is created. You can create global JavaScript objects
            // at this point. These objects persist throughout the lifetime of the view
            // and are available to all pages loaded by this view.

            //using (JSObject myObject = webControl.CreateGlobalJavascriptObject("myObject"))
            //    // Add a custom method to the global object and bind to it.
            //    myObject.BindAsync(onImageLoaded);
        }

        private void OnDocumentReady(object sender, DocumentReadyEventArgs e)
        {
            // When ReadyState is Ready, you can execute JavaScript against
            // the DOM but all resources are not yet loaded. Wait for Loaded.
            if (e.ReadyState == DocumentReadyState.Ready)
                return;

            // Get a screenshot of the view.
            GetScreenshot("wpf_screenshot_before.png");

            // Print the page's HTML source.
            PrintPageHTML();

            // Asynchronously execute some script on the page,
            // that will change the source of an image and wait
            // for the image load to complete.
            webControl.ExecuteJavascript(CHANGE_IMG_SRC);
        }

        private void onImageLoaded(object sender, JavascriptMethodEventArgs e)
        {
            Debug.Print(String.Format("IMG with id: '{0}' completed loading: {1}", e.Arguments[0], e.Arguments[1]));
            // Get another screenshot.
            GetScreenshot("wpf_screenshot_after.png");
        }

        // Any JavaScript errors or JavaScript console.log calls,
        // will call this method.
        private void OnConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Debug.Print("[Line: " + e.LineNumber + "] " + e.Message);
        }

        private void webControl_ShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e)
        {
            if (webControl == null)
                return;

            if (!webControl.IsLive)
                return;

            // An instance of our application's web window, that will
            // host the new view instance, either we wrap the created child view,
            // or we let the WebControl create a new underlying web-view.
            //MainWindow newWindow;

            // Treat popups differently. If IsPopup is true, the event is always
            // the result of 'window.open' (IsWindowOpen is also true, so no need to check it).
            // Our application does not recognize user defined, non-standard specs. 
            // Therefore child views opened with non-standard specs, will not be presented as 
            // popups but as regular new windows (still wrapping the child view however -- see below).
            if (e.IsPopup && !e.IsUserSpecsOnly)
            {
                // JSWindowOpenSpecs.InitialPosition indicates screen coordinates.
                Int32Rect screenRect = e.Specs.InitialPosition.GetInt32Rect();

                // Set the created native view as the underlying view of the
                // WebControl. This will maintain the relationship between
                // the parent view and the child, usually required when the new view
                // is the result of 'window.open' (JS can access the parent window through
                // 'window.opener'; the parent window can manipulate the child through the 'window'
                // object returned from the 'window.open' call).

                //newWindow = new MainWindow(e.NewViewInstance);
                //// Do not show in the taskbar.
                //newWindow.ShowInTaskbar = false;
                //newWindow.Topmost = true;
                //// Set a border-style to indicate a popup.
                //newWindow.WindowStyle = WindowStyle.ToolWindow;
                //// Set resizing mode depending on the indicated specs.
                //newWindow.ResizeMode = e.Specs.Resizable ? ResizeMode.CanResizeWithGrip : ResizeMode.NoResize;

                // If the caller has not indicated a valid size for the new popup window,
                // let it be opened with the default size specified at design time.
                if ((screenRect.Width > 0) && (screenRect.Height > 0))
                {
                    // The indicated size, is client size.
                    double horizontalBorderHeight = SystemParameters.ResizeFrameHorizontalBorderHeight;
                    double verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;
                    double captionHeight = SystemParameters.CaptionHeight;

                    // Set the indicated size.
                    //newWindow.Width = screenRect.Width + (verticalBorderWidth * 2);
                    //newWindow.Height = screenRect.Height + captionHeight + (horizontalBorderHeight * 2);
                }

                // Show the window.
                //newWindow.Show();

                // If the caller has not indicated a valid position for the new popup window,
                // let it be opened in the default position specified at design time.
                if ((screenRect.Y >= 0) && (screenRect.X >= 0))
                {
                    // Move it to the indicated coordinates.
                    //newWindow.Top = screenRect.Y;
                    //newWindow.Left = screenRect.X;
                }
            }
            else if (e.IsWindowOpen || e.IsPost)
            {
                // No specs or only non-standard specs were specified, but the event is still 
                // the result of 'window.open' or of an HTML form with target="_blank" and method="post".
                // We will open a normal window but we will still wrap the new native child view, 
                // maintaining its relationship with the parent window.

                //newWindow = new MainWindow(e.NewViewInstance);
                // Show the window.
                //newWindow.Show();
            }
            else
            {
                // The event is not the result of 'window.open' or of an HTML form with target="_blank" 
                // and method="post"., therefore it's most probably the result of a link with target='_blank'. 
                // We will not be wrapping the created view; we let the WebControl hosted in ChildWindow 
                // create its own underlying view. Setting Cancel to true tells the core to destroy the 
                // created child view.
                //
                // Why don't we always wrap the native view passed to ShowCreatedWebView?
                //
                // - In order to maintain the relationship with their parent view,
                // child views execute and render under the same process (awesomium_process)
                // as their parent view. If for any reason this child process crashes,
                // all views related to it will be affected. When maintaining a parent-child 
                // relationship is not important, we prefer taking advantage of the isolated process 
                // architecture of Awesomium and let each view be rendered in a separate process.
                e.Cancel = true;
                // Note that we only explicitly navigate to the target URL, when a new view is 
                // about to be created, not when we wrap the created child view. This is because 
                // navigation to the target URL (if any), is already queued on created child views. 
                // We must not interrupt this navigation as we would still be breaking the parent-child
                // relationship.

                //newWindow = new MainWindow(e.TargetURL);
                // Show the window.
                //newWindow.Show();
            }
        }

        private void webControl_WindowClose(object sender, WindowCloseEventArgs e)
        {
            // The page called 'window.close'. If the call
            // comes from a frame, ignore it.
            //if (!e.IsCalledFromFrame)
            //this.Close();
        }
        #endregion

        #region Properties
        // This will be set to the target URL, when this window does not
        // host a created child view. The WebControl, is bound to this property.
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(MapView),
            new FrameworkPropertyMetadata(null));


        // This will be set to the created child view that the WebControl will wrap,
        // when ShowCreatedWebView is the result of 'window.open'. The WebControl, 
        // is bound to this property.
        public IntPtr NativeView
        {
            get { return (IntPtr)GetValue(NativeViewProperty); }
            private set { this.SetValue(NativeViewPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey NativeViewPropertyKey =
            DependencyProperty.RegisterReadOnly("NativeView", typeof(IntPtr), typeof(MapView),
            new FrameworkPropertyMetadata(IntPtr.Zero));

        /// <summary>
        /// Identifies the <see cref="NativeView"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NativeViewProperty =
            NativeViewPropertyKey.DependencyProperty;

        // The visibility of the address bar and status bar, depends
        // on the value of this property. We set this to false when
        // the window wraps a WebControl that is the result of JavaScript
        // 'window.open'.
        public bool IsRegularWindow
        {
            get { return (bool)GetValue(IsRegularWindowProperty); }
            private set { this.SetValue(IsRegularWindowPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsRegularWindowPropertyKey =
            DependencyProperty.RegisterReadOnly("IsRegularWindow", typeof(bool), typeof(MapView),
            new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsRegularWindow"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsRegularWindowProperty =
            IsRegularWindowPropertyKey.DependencyProperty;

        #endregion
    }

    public class Browser : IDisposable
    {
        private class Interceptor : IResourceInterceptor
        {
            public bool OnFilterNavigation(NavigationRequest request)
            {
                return false;
            }

            public ResourceResponse OnRequest(ResourceRequest request)
            {
                if (request.Method != "GET")
                    return null;

                request.AppendExtraHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                request.AppendExtraHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.111 Safari/537.36");
                request.AppendExtraHeader("Accept-Language", "en-US,en;q=0.8");
                return null;
            }
        }

        private static Thread _thread;
        private static bool _coreIsRunning;
        private static WebSession _session;
        string _renderedHtml = "";
        bool _renderingDone;
        WebView _view;
        JSObject _jsObj;

        private static void AwesomiumThread()
        {
            if (WebCore.IsInitialized == false & _coreIsRunning == false)
            {
                WebCore.Initialize(new WebConfig { LogLevel = LogLevel.None }, false);
                WebCore.Run((s, e) =>
                {
                    WebCore.ResourceInterceptor = new Interceptor();
                    _coreIsRunning = true;
                });
            }
        }

        /// <summary>
        /// Call this to shutdown the dedicated awesomium thread and destroy any WebViews created by the WebCore.
        /// Call it when your app is closing.
        /// Once, shutdown, app restart is needed to use WebCore again.
        /// </summary>
        public static void StopWebCore()
        {
            if (_session != null)
            {
                WebCore.DoWork<object>(() =>
                {
                    _session.Dispose();
                    _session = null;
                    return null;
                });
            }
            WebCore.Shutdown();
            _thread = null;
            _coreIsRunning = false;
        }

        /// <summary>
        /// Call this to start awesomium webcore on it's own dedicated thread.
        /// You can call this on app start but it's not needed as it will be called automatically when you create an instance of this class.
        /// </summary>
        public static void StartWebCore()
        {
            if (WebCore.IsInitialized == false & _coreIsRunning == false & (_thread == null))
            {
                _thread = new Thread(AwesomiumThread);
                _thread.Start();
            }
        }

        public static void ReduceWebCoreMemory()
        {
            WebCore.ReleaseMemory();
        }

        /// <summary>
        /// Create a new Browser class.
        /// Don't forget to either use a USING statement or call DISPOSE after use.
        /// </summary>
        public Browser()
        {
            StartWebCore();
            InitSession();
            SetNewView();
        }

        private static void InitSession()
        {
            if (_session == null)
            {
                //SRC = '';

                //function SetSRC() {
                //    SRC = document.documentElement.outerHTML;
                //};

                //function RemoveVideos() {
                //    var element = document.getElementsByTagName('video');
                //    for (index = element.length - 1; index >= 0; index--) {
                //        element[index].parentNode.removeChild(element[index]);
                //    };
                //};

                //if (window.addEventListener) {
                //    window.addEventListener('DOMContentLoaded', RemoveVideos, false);
                //    window.addEventListener("load", SetSRC, false);
                //};

                //setTimeout(function() {
                //    _jsObj.SendSRC(document.documentElement.outerHTML);
                //}, 10000);

                _session = WebCore.DoWork(() => { return WebCore.CreateWebSession(new WebPreferences { LoadImagesAutomatically = false, LocalStorage = false, Plugins = false, RemoteFonts = false, WebAudio = false, CanScriptsOpenWindows = false, DefaultEncoding = "utf-8", MaxHttpCacheStorage = 64000000, UserScript = "function SetSRC(){SRC=document.documentElement.outerHTML}function RemoveVideos(){var e=document.getElementsByTagName('video');for(index=e.length-1;index>=0;index--)e[index].parentNode.removeChild(e[index])}SRC='',window.addEventListener&&(window.addEventListener('DOMContentLoaded',RemoveVideos,!1),window.addEventListener('load',SetSRC,!1)),setTimeout(function(){_jsObj.SendSRC(document.documentElement.outerHTML)},1e4);" }); });
            }

        }

        private void SetNewView()
        {
            if ((_view != null))
            {
                _view.LoadingFrameFailed -= null;
                _view.DocumentReady -= null;
                WebCore.DoWork<object>(() =>
                {
                    if ((_view != null))
                    {
                        _view.Dispose();
                    }
                    return null;
                });
                _view = null;
            }
            _view = WebCore.DoWork(() => { return WebCore.CreateWebView(1000, 500, _session, WebViewType.Offscreen); });

            _view.DocumentReady += (s, e) =>
            {
                if ((_jsObj == null))
                {
                    _view.SynchronousMessageTimeout = 3000;
                    _jsObj = _view.CreateGlobalJavascriptObject("_jsObj");
                    _jsObj.BindAsync("SendSRC", (sen, eve) =>
                    {
                        if (!_renderingDone)
                        {
                            _renderedHtml = eve.Arguments[0].ToString();
                            Debug.WriteLine("SRC READY: " + _view.Source.ToString());
                            _renderingDone = true;
                        }
                    });
                }
                if (_renderingDone == false & _view.IsDocumentReady == true)
                {
                    _renderedHtml = _view.ExecuteJavascriptWithResult("SRC").ToString();
                    if (!string.IsNullOrEmpty(_renderedHtml))
                    {
                        Debug.WriteLine("SRC READY: " + _view.Source.ToString());
                        _renderingDone = true;
                    }
                }
            };

            _view.LoadingFrameFailed += (s, e) =>
            {
                if (e.IsMainFrame)
                {
                    _renderedHtml = "";
                    _renderingDone = true;
                }
            };
        }

        /// <summary>
        /// Returns a browser rendered final HTML source for a given url, including dynamic content such as javascript.
        /// </summary>
        /// <param name="url">A properly formatted valid url in the form of "http://website.com/"</param>
        /// <returns>Fully rendered valid HTML markup.</returns>
        public string GetRenderedHtml(string url)
        {
            while (!(_coreIsRunning))
            {
                Task.Delay(100).Wait();
            }

            _renderingDone = false;
            _view.Invoke(new Action(() => {
                while (!(_view.IsLive))
                {
                    Task.Delay(100).Wait();
                }
                Debug.WriteLine("INVOKING: " + url);
                _view.Source = url.ToUri();
            }));

            System.DateTime startTime = System.DateTime.UtcNow;
            while (_renderingDone != true)
            {
                if (System.DateTime.UtcNow.Subtract(startTime).TotalSeconds >= 15)
                {
                    _view.Invoke(new Action(() => { _view.Stop(); }));
                    Debug.WriteLine("TERMINATING: " + url);
                    break; // TODO: might not be correct. Was : Exit Do
                }
                Task.Delay(300).Wait();
            }

            Debug.WriteLine("RENDER DONE: " + url);

            if (string.IsNullOrEmpty(_renderedHtml) | _renderedHtml.Equals("undefined", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("RENDER OUTPUT NULL: " + url);
                throw new Exception("Rendering failed!");
            }

            //Dim doc As New HtmlAgilityPack.HtmlDocument()
            //doc.LoadHtml(_renderedHtml)
            //Dim body As HtmlNode = doc.DocumentNode.SelectSingleNode("//*[name() = 'body']")
            //If IsNothing(body) Then
            //    Debug.WriteLine("NO BODY TAG: " + url)
            //    Throw New Exception("No body tag found!")
            //End If
            //If String.IsNullOrWhiteSpace(body.InnerHtml) Then
            //    Debug.WriteLine("EMPTY BODY: " + url)
            //    Throw New Exception("Empty body content!")
            //End If

            return _renderedHtml;
        }

        #region "IDisposable Support"
        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    _view.LoadingFrameFailed -= null;
                    _view.DocumentReady -= null;
                    WebCore.QueueWork(() =>
                    {
                        if ((_view != null))
                        {
                            _view.Dispose();
                            _view = null;
                        }
                        if ((_jsObj != null))
                        {
                            _jsObj.Dispose();
                            _jsObj = null;
                        }
                    });
                }
                _renderedHtml = null;
                _renderingDone = false;
            }
            this._disposedValue = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
