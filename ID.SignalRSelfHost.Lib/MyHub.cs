using System;
using Microsoft.AspNet.SignalR;

namespace ID.SignalRSelfHost.Lib
{
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
}