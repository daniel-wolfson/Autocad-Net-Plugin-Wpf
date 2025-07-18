using Microsoft.Owin.Cors;
using Owin;

namespace ID.SignalRSelfHost.Lib
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
}