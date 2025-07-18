using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using ID.SignalRSelfHost.Lib;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

[assembly: ExtensionApplication(typeof(SelfHost))]
[assembly: CommandClass(typeof(SelfHost))]
namespace ID.SignalRSelfHost.Lib
{
    public class SelfHost : IExtensionApplication
    {
        private static IDisposable server;

        [CommandMethod("SelfHost", CommandFlags.Modal)]
        public void Start()
        {
            string url = "http://localhost:8080";
            server = WebApp.Start<Startup>(url);
            
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Application.DocumentManager.GetDocument(db);
            doc.Editor.WriteMessage("server running on { 0}", url);
        }

        public void Initialize()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Document doc = Application.DocumentManager.GetDocument(db);
            string url = "http://localhost:8080";

            try
            {
                server = WebApp.Start<Startup>(url);
                doc.Editor.WriteMessage("server running on {0}", url);
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage("server falied on {0}", url);
            }
        }

        public void Terminate()
        {
            server?.Dispose();
        }



        //public void Initialize()
        //{
        //    string url = "http://MapIt:8080";
        //    server = WebApp.Start<Startup>(url);
        //    acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("server running on { 0}", url);
        //}

        //public void Terminate()
        //{
        //    server?.Dispose();
        //    acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("SelfHost closed.");
        //}

        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                //app.UseCors(CorsOptions.AllowAll);
                app.MapSignalR();
            }
        }

        public class AcadMapHub : Hub
        {
            //Test method, not used in this blog
            public void Send(string name, string message)
            {
                Clients.All.addMessage(name, message);
            }

            //AutoCAD calls this method by sending an 
            //AcadCommandTrack object to AcadMapHub
            public void SendAcadMessage(AcadCommandTrack cmdTrack)
            {
                //Write to console, in order to see the server does
                //receive calls from SignalR client
                Console.Write("Received data: " + cmdTrack.ToString());

                //AcadMapHub calls all connected clients where an Action
                //named as "getAcadMessage" (function in JavScript)
                //will do something on the client side
                Clients.All.getAcadMessage(cmdTrack.ToString());

                Console.WriteLine("...data has been sent to client.");
                Console.WriteLine("Press any key to exit...");
            }
        }

        public class AcadCommandTrack
        {
            public string UserName { set; get; }
            public string ComputerName { set; get; }
            public string CommandName { set; get; }
            public DateTime CmdExecTime { set; get; }
            public string DwgFileName { set; get; }

            public override string ToString()
            {
                return "Command \"" + CommandName + "\"" +
                       " executed in drawing \"" + DwgFileName + "\"" +
                       " at " + CmdExecTime.ToLongTimeString() +
                       " on computer \"" + ComputerName + "\"" +
                       " by \"" + UserName + "\"";
            }

            public AcadCommandTrack()
            {
                UserName = "None";
                ComputerName = "None";
                CommandName = "None";
                CmdExecTime = DateTime.Now;
                DwgFileName = "";
            }
        }
    }
}