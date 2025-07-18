using System.Diagnostics;
using MapIt.WebApi.Tests;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace MapIt.WebApi.Tests
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            //HttpConfiguration config = new HttpConfiguration();
            ////config.Services.Replace(typeof(IAssembliesResolver), new TestWebApiResolver());

            //UnityConfig.Register(config);

            //WebApiConfig.Register(config);
            //Debug.WriteLine("WebApiConfig");

            //app.UseWebApi(config);

            ////config.DependencyResolver = new UnityDependencyResolver(new UnityContainer()); 
            ////UnityHelpers.GetConfiguredContainer())

            //var appPath = HostingEnvironment.MapPath(@"~/bin");
            ////Utilities.LoadNativeAssemblies(appPath);

            //app.Use(typeof(LogMiddleware));

            //var corsAttr = new EnableCorsAttribute("http://example.com", "*", "*");
            app.UseCors(CorsOptions.AllowAll);

            app.MapSignalR();
            app.RunSignalR(new HubConfiguration()
            {
                EnableDetailedErrors = true
                //Resolver = resolver
            });

            //app.Map("/signalr", map =>
            //{
            //    map.Use(typeof(LogMiddleware));

            //    //var corsAttr = new EnableCorsAttribute("http://example.com", "*", "*");
            //    map.UseCors(CorsOptions.AllowAll);

            //    map.MapSignalR();
            //    map.RunSignalR(new HubConfiguration()
            //    {
            //        EnableDetailedErrors = true
            //        //Resolver = resolver
            //    });
            //    //SignalRConfig();
            //});
        }

        private void SignalRConfig()
        {
            string url = "http://localhost:43210";
            using (WebApp.Start(url))
            {
                Debug.WriteLine("server running on {0}", url);
            }
        }
    }
}