using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ID.Infrastructure.Extensions
{
    public static class HttpActionResultExtentions
    {
        public static IHttpActionResult AddHeader(this IHttpActionResult result, string name, IEnumerable<string> values)
            => new HeaderActionResult(result, name, values);

        public static IHttpActionResult AddHeader(this IHttpActionResult result, string content, string headerName, IEnumerable<string> headerValues)
            => new RawJsonActionResult(content, headerName, headerValues);

        private class HeaderActionResult : IHttpActionResult
        {
            private readonly IHttpActionResult actionResult;
            private readonly Tuple<string, IEnumerable<string>> header;

            public HeaderActionResult(IHttpActionResult actionResult, string headerName, IEnumerable<string> headerValues)
            {
                this.actionResult = actionResult;
                header = Tuple.Create(headerName, headerValues);
            }

            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = await actionResult.ExecuteAsync(cancellationToken);
                response.Headers.Add(header.Item1, header.Item2);
                return response;
            }
        }

        private class RawJsonActionResult : IHttpActionResult
        {
            private readonly string _jsonString;
            private readonly Tuple<string, IEnumerable<string>> header;

            public RawJsonActionResult(string jsonString, string headerName, IEnumerable<string> headerValues)
            {
                _jsonString = jsonString;
                header = Tuple.Create(headerName, headerValues);
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var content = new StringContent(_jsonString);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
                response.Headers.Add(header.Item1, header.Item2);
                return Task.FromResult(response);
            }
        }
    }
}
