using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Awesomium.Core;

//Imports HtmlAgilityPack

namespace Intellidesk.AcadNet.Test
{
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
            while (!_coreIsRunning)
            {
                Task.Delay(100).Wait();
            }

            _renderingDone = false;
            _view.Invoke(new Action(() => {
                    while (!_view.IsLive)
                    {
                        Task.Delay(100).Wait();
                    }
                    Debug.WriteLine("INVOKING: " + url);
                    _view.Source = url.ToUri();
            }));

            DateTime startTime = DateTime.UtcNow;
            while (_renderingDone != true)
            {
                if (DateTime.UtcNow.Subtract(startTime).TotalSeconds >= 15)
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
