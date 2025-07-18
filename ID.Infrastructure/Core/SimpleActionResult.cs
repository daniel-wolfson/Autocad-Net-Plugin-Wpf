using System.Net;

namespace ID.Infrastructure.General
{
    public class SimpleActionResult
    {
        public object Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public object ActionResult { get; set; }
    }
}
