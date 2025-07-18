using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.General;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Services.Extentions;
using Intellidesk.AcadNet.Services.Jobs;
using Microsoft.Owin.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Utils = Autodesk.AutoCAD.Internal.Utils;

namespace Intellidesk.AcadNet
{
    public class AcadNetHttpServerHost
    {
        private static IDisposable _signalRSelfHost;
        public static AcadNetSignalRClientHost SignalRClientHost { get; private set; }

        #region privates properties

        private Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        private Editor _ed => acadApp.DocumentManager.MdiActiveDocument.Editor;
        private Editor Ed => Doc.Editor;
        private Database Db => Doc.Database; //Db = HostApplicationServices.WorkingDatabase; Doc = Application.DocumentManager.GetDocument(Db); 
        private IPluginSettings _pluginSettings;
        private IPluginSettings PluginSettings
        {
            get { return _pluginSettings ?? (_pluginSettings = Plugin.GetService<IPluginSettings>()); }
        }

        #endregion privates

        public async Task<bool> InitializeWebApiAsync()
        {
            bool result = await Task.Run(() =>
            {
                try
                {
                    _ed.WriteMessage($"{PluginSettings.Prompt}Intellidesk initializing HTTP server self hosting...");
                    //_httpServer = CreateHttpSelfHostServer(AcadApiSelfHost);
                    //_httpServer.OpenAsync().Wait();
                    _ed.WriteMessage($"{AcadApiSelfHost} completed.\n");
                    return true;

                }
                catch (Exception)
                {
                    _ed.WriteMessage($"{AcadApiSelfHost} Server failed.\n");
                    return false;
                }
            });
            return result;
        }

        public async Task<bool> InitializeJobSchedulerAsync()
        {
            bool result = await Task.Run(() =>
            {
                try
                {
                    _ed.WriteMessage($"{PluginSettings.Prompt}Intellidesk initializing IDScheduler...");
                    EmailScheduler.Start();
                    _ed.WriteMessage($"{AcadApiSelfHost} completed.\n");
                    return true;
                }
                catch (Exception)
                {
                    _ed.WriteMessage($"{AcadApiSelfHost} Server failed.\n");
                    return false;
                }
            });
            return result;
        }

        public async Task<bool> InitializeSignalRAsync()
        {
            bool result = true;
            if (SignalRClientHost != null) return result;

            try
            {
                _ed.WriteMessage($"{PluginSettings.Prompt}Intellidesk initializing SignalRClientHost...");

                if (SignalRClientHost != null)
                {
                    SignalRClientHost.Terminate();
                    SignalRClientHost = null;
                }

                SignalRClientHost = new AcadNetSignalRClientHost();

                if (ProcessManager.IsProcessRunning("ID.SignalRSelfHost"))
                {
                    result = await SignalRClientHost.InitConnectionAsync(t =>
                        {
                            Ed.WriteMessage($"{PluginSettings.Prompt} SignalR hub connected");
                        });
                }
                else
                {
                    _ed.WriteMessage("process ID.SignalRSelfHost not found!\n");
                    return true;
                }
            }
            catch (Exception)
            {
                SignalRClientHost = null;
                _ed.WriteMessage(" failed.\n");
                result = false;
            }
            return result;
        }

        public async Task<bool> ResetSignalRAsync()
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                ProgressMeter pm = new ProgressMeter();

                pm.Start("Send initializing ...");
                pm.SetLimit(2);
                Thread.Sleep(1000);

                pm.MeterProgress();

                bool initResult = await AcadNetManager.HttpServerHostInitializer.InitializeSignalRAsync();
                if (initResult)
                {
                    AcadNetSignalRClientHost signalRClientHost = AcadNetHttpServerHost.SignalRClientHost;
                    if (!signalRClientHost.IsConnected)
                    {
                        string res = signalRClientHost.LoadHostAsync().Result;
                        if (res != null)
                        {
                            pm = new ProgressMeter();
                            pm.Start($"{PluginSettings.Prompt} SignalR hub recreated, connected");
                            pm.SetLimit(1);
                            Thread.Sleep(1000);
                            pm.MeterProgress();
                            pm.Stop();
                            pm.Dispose();
                        }

                        return await Task.FromResult(res != null);
                    }
                    return await Task.FromResult(true);
                }

                if (!pm.IsDisposed)
                {
                    pm.Stop();
                    pm.Dispose();
                }

                return await Task.FromResult(false);
            }
        }

        public static bool IsSignalRClientHostConnected
        {
            get { return SignalRClientHost != null && SignalRClientHost.IsConnected; }
        }

        public void Terminate()
        {
            HttpServer?.Dispose();
            _signalRSelfHost?.Dispose();
        }

        #region temp

        public static HttpSelfHostServer HttpServer { get; }
        public string AcadApiSelfHost { get; }
        private void OnHostMessage1(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Ed.WriteMessageAsync(message);
        }
        private const string ChatHostUrl = "localhost:8080"; //localhost:8080 Settings.Default.IntelliDeskHost;
        public void InitializeSignalRSelfHost()
        {
            try
            {
                _ed.WriteMessage($"{PluginSettings.Prompt}Intellidesk building bridge...");
                _signalRSelfHost = WebApp.Start<AcadNetStartUp>(ChatHostUrl);
                _ed.WriteMessage($"{ChatHostUrl} completed.\n");
            }
            catch (Exception)
            {
                _signalRSelfHost?.Dispose();
                _signalRSelfHost = null;
                _ed.WriteMessage($"{ChatHostUrl} Server failed.\n");
            }
            finally
            {
                Utils.PostCommandPrompt();
            }
        }
        public static bool IsSignalRClientHostExist()
        {
            if (!ProcessManager.IsProcessRunning("ID.SignalRSelfHost"))
            {
                acadApp.ShowAlertDialog("Host process not found! Need execute file: ID.SignalRSelfHost.exe");
                //ProcessManager.RunAsync("ID.SignalRSelfHost");
                return false;
            }
            return true;
        }
        private HttpSelfHostServer CreateHttpSelfHostServer(string baseUrl)
        {
            HttpSelfHostConfiguration config = ConfigurateHost(baseUrl);
            HttpSelfHostServer server = new HttpSelfHostServer(config);
            return server;
        }
        private HttpSelfHostConfiguration ConfigurateHost(string baseUrl)
        {
            // http:/intellidesk/acadapi
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseUrl);
            config.Routes.MapHttpRoute(
                name: "SelfHost",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );
            return config;
        }

        #endregion
    }
}
