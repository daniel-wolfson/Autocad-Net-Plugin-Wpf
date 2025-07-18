using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Intellidesk.AcadNet.Helpers
{
    public class MapItHeaderActionResult<TContent> : IHttpActionResult
    {
        private DateTime _date;
        private TContent _content;
        private HttpRequestMessage _request;


        public MapItHeaderActionResult(DateTime date, TContent content, HttpRequestMessage request)
        {
            _date = date;
            _content = content;
            _request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = _request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("MapIt", Guid.NewGuid().ToString());
            response.Content = new ObjectContent<TContent>(_content, new JsonMediaTypeFormatter());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return Task.FromResult(response);
        }
    }
}