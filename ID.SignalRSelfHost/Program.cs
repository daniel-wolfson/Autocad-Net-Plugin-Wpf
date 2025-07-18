using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Intellidesk.SignalRSelfHost
{
    public class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        static void Main(string[] args)
        {
            if (IsCurrentProcessExist()) return;

            IntPtr hWnd = GetConsoleWindow();
            if (hWnd != IntPtr.Zero)
                ShowWindow(hWnd, 0);

            string url = ConfigurationManager.AppSettings["url"];
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine(@"server running on {0}", url);
                Console.WriteLine(@"Press any key to exit...");
                Console.ReadLine();
            }
        }

        private static bool IsCurrentProcessExist()
        {
            Process[] processes = Process.GetProcesses();
            Process currentProc = Process.GetCurrentProcess();
            //logger.LogDebug("Current proccess: {0}", currentProc.ProcessName);

            if (processes.Any(process => currentProc.ProcessName == process.ProcessName && currentProc.Id != process.Id))
            {
                Console.WriteLine($@"Another instance of process '{currentProc.ProcessName}' is already running");
                Console.WriteLine(@"Press any key to exit...");
                Console.ReadLine();
                return true;
            }
            return false;
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //var idProvider = new CustomUserIdProvider();
            GlobalHost.DependencyResolver.Register(typeof(IPluginSettings), () => PluginSettings.Build());
            //var corsPolicy = new CorsPolicy
            //{
            //    AllowAnyMethod = true,
            //    AllowAnyHeader = true
            //};
            //corsPolicy.Origins.Add("http://localhost:4200/");
            //var corsOptions = new CorsOptions
            //{
            //    PolicyProvider = new CorsPolicyProvider
            //    {
            //        PolicyResolver = context => Task.FromResult(corsPolicy)
            //    }
            //};
            //app.UseCors(corsOptions);
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();

            //app.Map("/signalr", map =>
            //{
            //    // Setup the CORS middleware to run before SignalR.
            //    // By default this will allow all origins. You can 
            //    // configure the set of origins and/or http verbs by
            //    // providing a cors options with a different policy.
            //    map.UseCors(CorsOptions.AllowAll);
            //    var hubConfiguration = new HubConfiguration
            //    {
            //        // You can enable JSONP by uncommenting line below.
            //        // JSONP requests are insecure but some older browsers (and some
            //        // versions of IE) require JSONP to work cross domain
            //        // EnableJSONP = true
            //    };
            //    // Run the SignalR pipeline. We're not using MapSignalR
            //    // since this branch already runs under the "/signalr"
            //    // path.
            //    map.RunSignalR(hubConfiguration);
            //});

        }
    }


    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            // your logic to fetch a user identifier goes here.
            var userId = request.User.Identity.Name; //MyCustomUserClass.FindUserId(request.User.Identity.Name);
            return userId;
        }
    }
}
