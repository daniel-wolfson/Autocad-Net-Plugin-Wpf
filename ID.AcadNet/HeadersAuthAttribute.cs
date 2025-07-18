using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Intellidesk.AcadNet
{
    public class HeadersAuthAttribute : AuthorizeAttribute
    {
        private const string UserIdHeader = "SRUserId";

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            if (string.IsNullOrEmpty(request.Headers[UserIdHeader]))
            {
                return false;
            }

            return true;
        }
    }
}