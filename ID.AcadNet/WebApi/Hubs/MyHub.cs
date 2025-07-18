using Microsoft.AspNet.SignalR;

namespace Intellidesk.AcadNet.WebApi.Hubs
{
    public class MyHub2 : Hub
    {
        public void Subscribe(string customerId)
        {
            Groups.Add(Context.ConnectionId, customerId);
        }

        public void Unsubscribe(string customerId)
        {
            Groups.Remove(Context.ConnectionId, customerId);
        }
    }

    public class NotificationHub : Hub
    {
        public void Send(string message)
        {
            Clients.All.newMessage(message);
        }
    }

}