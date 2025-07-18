using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.Resources.Properties;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Owin;
using System.Web.Http;

//[assembly: OwinStartup(typeof(Intellidesk.AcadNet.AcadNetWebApiStartUp))]
namespace Intellidesk.AcadNet
{
    public class AcadNetStartUp
    {
        // This method is required by Katana:
        public void Configuration(IAppBuilder app)
        {
            app.Map("/signalr", map =>
            {
                IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                //FileServerConfiguration(app);
                //HttpConfiguration hubConfiguration = ConfigureWebApi();
                //app.UseWebApi(hubConfiguration);

                // Setup the cors middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.

                ed.WriteMessage($"{pluginSettings.Prompt}Intellidesk initializing UseCors...");
                map.UseCors(CorsOptions.AllowAll);
                ed.WriteMessage("completed\n");

                var hubConfiguration = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    //EnableJSONP = true
                    EnableDetailedErrors = true
                };

                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch is already runs under the "/signalr"
                // path.

                ed.WriteMessage(pluginSettings.Prompt + "Intellidesk initializing SignalR...");
                map.RunSignalR(hubConfiguration);
                ed.WriteMessage("completed.\n");
            });
        }

        private void FileServerConfiguration(IAppBuilder app)
        {
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                EnableDefaultFiles = true,
                DefaultFilesOptions = { DefaultFileNames = { "../Web/Mapit.html" } },
                FileSystem = new PhysicalFileSystem("App_Data"),
                StaticFileOptions = { ContentTypeProvider = new CustomContentTypeProvider() }
            };
            app.UseFileServer(options);

            var lOptions = new StaticFileOptions
            {
                ContentTypeProvider = new CustomContentTypeProvider(),
                FileSystem = new PhysicalFileSystem("App_Data")
            };

            app.UseStaticFiles(lOptions);
        }

        private HttpConfiguration ConfigureWebApi()
        {
            var config = new HttpConfiguration();
            //config.EnableCors();
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "{controller}/{action}/{id}", //"api/{controller}/{id}", IntelliDeskApi
                new { id = RouteParameter.Optional });

            //config.Formatters.Clear();
            //config.Formatters.Add(new JsonMediaTypeFormatter());

            return config;
        }
    }

    public class AcadNetWebApiStartUp
    {
        readonly Editor _ed = Application.DocumentManager.MdiActiveDocument.Editor;
        readonly string _adressHost = Settings.Default.MapitWebHost;

        // This method is required by Katana:
        public void Configuration(IAppBuilder app)
        {
            //app.MapSignalR();
            app.Map("/acadapi", map =>
            {
                var webApiConfiguration = ConfigureWebApi();
                map.UseWebApi(webApiConfiguration);

                // Setup the cors middleware to run before SignalR.
                // By default this will allow all origins. You can 
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                map.UseCors(CorsOptions.AllowAll);
                _ed.WriteMessage("\nIntellidesk initializing UseCors...");

                var hubConfiguration = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    //EnableJSONP = true
                };

                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch is already runs under the "/signalr"
                // path.
                //map.RunSignalR(hubConfiguration);

                //IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
                //_ed.WriteMessage(pluginSettings.Prompt + "Intellidesk initializing signalr...");
                //map.MapSignalR(hubConfiguration);
                //_ed.WriteMessage(" completed.\n");
            });
        }

        private HttpConfiguration ConfigureWebApi()
        {
            var config = new HttpConfiguration();
            //config.EnableCors();
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "{controller}/{action}/{id}", //"api/{controller}/{id}", IntelliDeskApi
                new { id = RouteParameter.Optional });

            //config.Formatters.Clear();
            //config.Formatters.Add(new JsonMediaTypeFormatter());

            return config;
        }
    }

    public class CustomContentTypeProvider : FileExtensionContentTypeProvider
    {
        public CustomContentTypeProvider()
        {
            Mappings.Add(".json", "application/json");
        }
    }
}

