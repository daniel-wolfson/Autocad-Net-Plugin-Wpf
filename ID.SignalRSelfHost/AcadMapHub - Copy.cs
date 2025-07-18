using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using Intellidesk.Data.Models.Map;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Core;
using Intellidesk.Common.Core;
using Intellidesk.Infrastructure.Interfaces;

namespace Intellidesk.SignalRSelfHost
{
    [HubName("acadMapHub")]
    public class AcadMapHub : Hub
    {
        private readonly string _prompt = "Intellidesk server: ";
        //private static Dictionary<string, string> onLineClients = new Dictionary<string, string>();

        public void SendToUser(string userId, string message)
        {
            Clients.User(userId).onMessage(message);
        }
        public void SendToGroup(string groupname, string message)
        {
            Clients.Group(groupname).onMessage(message);
        }
        public void SendToAll(string name, string message)
        {
            Clients.All.onMessage(message);
        }

        //AutoCAD calls this method by sending an AcadCommandPass object to AcadMapHub
        public void SendToAcad(AcadCommandPass data)
        {
            //Write to console, in order to see the server does
            //receive calls from SignalR client
            Console.Write($"{_prompt}Received data: {data}");

            //AcadMapBridgeHub calls all connected clients where an Action
            //named as "getAcadMessage" (function in JavScript)
            //will do something on the client side
            //Clients.Group("mapit").onAcadPass(data);
            Clients.All.onAcadPass(JsonConvert.SerializeObject(data));

            Console.WriteLine($"{_prompt}...data has been sent to client(onAcadPass)");
            //Console.WriteLine("Press any key to exit...");
        }
        public void SendToFolderOpen(MapMarkerElement data)
        {
            IPluginSettings pluginSettings = (IPluginSettings)GlobalHost.DependencyResolver.GetService(typeof(IPluginSettings));
            var paths = pluginSettings.IncludeFolders.ToArray();
            SimpleActionResult simpleResult = Files.FindPath(paths, data.FolderName, null);

            if (simpleResult.StatusCode == HttpStatusCode.Found)
            {
                //Process.Start(simpleResult.ActionResult.ToString());
                Process.Start(new ProcessStartInfo()
                {
                    FileName = simpleResult.ActionResult.ToString(),
                    Verb = "open"
                });
            }

            //else
            //command.Cancel(new ErrorNotifyArgs(simpleResult.Message.ToString()));
            
            Console.WriteLine($"{_prompt}sent to all clients the command 'SendToFolderOpen': {data.FolderName}, {data.FileName}");
        }

        public void SendToFileOpen(MapMarkerElement data)
        {
            Clients.All.onFileOpen(data);
            //Clients.Group("acad").FileOpen(data);
            Console.WriteLine($"{_prompt}sent to all clients the command 'SendToFileOpen': {data.FolderName}, {data.FileName}");
        }

        public void SendToDisplayPoint(MapMarkerElement data)
        {
            Clients.All.onDisplayPoint(data);
            //Clients.Group("acad").FileOpen(data);
            Console.WriteLine($"{_prompt}sent to all clients the command 'onDisplayPoint': {data.Longitude}, {data.Latitude}");
        }
        public void AddConnection(string clientType)
        {
            //IHubContext context = GlobalHost.ConnectionManager.GetHubContext<AcadMapHub>();
            //if (!onLineClients.ContainsKey(Context.ConnectionId))
            {
                string name = Environment.UserName; //Context.User.Identity.Name;
                //onLineClients.Add(Context.ConnectionId, $"{name} ({clientType})");
                Groups.Add(Context.ConnectionId, clientType);
                Clients.Others.onMessage($"Intellidesk server: AcadMapHub connecting: {name}");
                Clients.Caller.onMessage(Context.ConnectionId, name, "");
                Console.WriteLine($"{_prompt}AcadMapHub: clientType={clientType} user={name} connected");
            }
        }

        //public void RemoveConnection(string customerId)
        //{
        //    if (!onLineClients.ContainsKey(Context.ConnectionId))
        //    {
        //        onLineClients.Add(Context.ConnectionId, $"{Context.User.Identity.Name} ({clientType})");
        //        Groups.Remove(Context.ConnectionId, customerId);
        //    }
        //}

        public override Task OnConnected()
        {
            var clientType = Context.QueryString.Get("intellidesk-clienttype");
            string name = Environment.UserName; //Context.User.Identity.Name;
            string connectionId = Context.ConnectionId + "." + name;
            Clients.Others.onMessage(name, "connected");
            Clients.Caller.onMessage(name, "connected");
            Groups.Add(connectionId, clientType);
            Console.WriteLine($"{_prompt}AcadMapHub: client group={clientType} user={name} connected");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var clientType = Context.QueryString.Get("intellidesk-clienttype");
            string name = Environment.UserName; //Context.User.Identity.Name;
            string connectionId = Context.ConnectionId + "." + name;
            Clients.Others.onMessage(name, "disconnected");
            Clients.Caller.onMessage(name, "disconnected");
            Groups.Remove(connectionId, clientType);
            Console.WriteLine(stopCalled
                ? $"{_prompt}AcadMapHub: client group={clientType} user={name} explicitly closed the connection"
                : $"{_prompt}AcadMapHub: client group={clientType} user={name} timed out");
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
    }
}