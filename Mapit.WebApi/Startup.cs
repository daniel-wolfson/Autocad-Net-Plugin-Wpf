using ID.Infrastructure;
using Intellidesk.Data.Auth;
using Microsoft.Owin;
using Owin;
using System.Web.Hosting;
using System.Web.Http;

[assembly: OwinStartup(typeof(MapIt.WebApi.Startup))]
namespace MapIt.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //var EnvironmentName = System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment ? "development" : "production";
            Plugin.Create(HostingEnvironment.MapPath(@"~/App_Data")).Register(Plugin.Container);

            UnityConfig.Register(GlobalConfiguration.Configuration, Plugin.Container);

            app.CreatePerOwinContext(AppDbContext.Create);

            app.CreatePerOwinContext<AppUserManager>(AppUserManager.Create);

            //app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            app.UseOAuthBearerTokens();

            app.UseJsonFormat(GlobalConfiguration.Configuration);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            WebApiConfig.Register(GlobalConfiguration.Configuration);

            app.UseWebApi(GlobalConfiguration.Configuration);

            //GlobalConfiguration.Configuration.EnsureInitialized();
        }
    }
}
