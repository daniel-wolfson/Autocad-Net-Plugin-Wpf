using System.Web.Http;
using System.Web.Http.SelfHost;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Intellidesk.AcadNet.Services.WebApi;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: ExtensionApplication(typeof(HttpServerHostInitializer))]
namespace Intellidesk.AcadNet.Services.WebApi
{
    /// <summary> HttpServerHostInitializer </summary>
    public class HttpServerHostInitializer : IExtensionApplication 
    {
        static HttpSelfHostServer _httpServer;

        #region IExtensionApplication Members

        public void Initialize()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            try
            {
                ed.WriteMessage("\nInitializing HTTP server hosting...");
                _httpServer = CreateHttpSelfHostServer("http://IntelliDesk"); //http://localhost:43210
                _httpServer.OpenAsync().Wait();
                ed.WriteMessage("completed.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("failed:\n");
                ed.WriteMessage(ex.Message);
            }
            finally
            {
                Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
            }
        }

        public void Terminate()
        {
            if (_httpServer != null)
                _httpServer.Dispose();
        }

        #endregion

        #region private methods

        private HttpSelfHostServer CreateHttpSelfHostServer(string baseUrl)
        {
            HttpSelfHostConfiguration config = ConfigurateHost(baseUrl);
            HttpSelfHostServer server = new HttpSelfHostServer(config);
            return server;
        }

        private HttpSelfHostConfiguration ConfigurateHost(string baseUrl)
        {
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseUrl);

            config.Routes.MapHttpRoute(
                name: "Api with action",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
                name: "Default Api",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            return config;
        }

        #endregion

        #region HOST methods sample

        //public void SelfHostInitialize()
        //{
        //    Document dwg = Application.DocumentManager.MdiActiveDocument;
        //    Editor Ed = dwg.Editor;

        //    try
        //    {
        //        Ed.WriteMessage("\nInitializing HTTP server hosting...");

        //        _httpServer = CreateHttpSelfHostServer("http://localhost:43210");
        //        _httpServer.OpenAsync().Wait();
        //        Ed.WriteMessage("completed.");
        //        Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        //    }

        //    catch (System.Exception ex)
        //    {
        //        Ed.WriteMessage("failed:\n");
        //        Ed.WriteMessage(ex.Message);
        //        Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
        //    }
        //}

        //private HttpSelfHostServer CreateHttpSelfHostServer(string baseUrl)
        //{
        //    HttpSelfHostConfiguration config = ConfigurateHost(baseUrl);
        //    HttpSelfHostServer server = new HttpSelfHostServer(config);
        //    return server;
        //}

        //private HttpSelfHostConfiguration ConfigurateHost(string baseUrl)
        //{
        //    HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseUrl);
        //    config.Routes.MapHttpRoute(
        //        name: "DefaultApi",
        //        routeTemplate: "api/{controller}/{id}",
        //        defaults: new { id = RouteParameter.Optional }
        //        );
        //    return config;
        //}

        #endregion
    }
}
