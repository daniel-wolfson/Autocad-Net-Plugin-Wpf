using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ID.Infrastructure.Core
{
    /// <summary> General named rest http client </summary>
    public class GeneralHttpClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ApiServiceNames _namedhttpClient;
        private readonly ILogger _logger;

        #region Public properties

        public string HttpClientName => _namedhttpClient.ToString();
        public Uri BaseAddress => _httpClient.BaseAddress;
        public string RequestUrl { get; private set; }
        public ApiResponse ResponseDetails { get; private set; }
        public long MaxResponseContentBufferSize => _httpClient.MaxResponseContentBufferSize;
        public TimeSpan Timeout => _httpClient.Timeout;
        public HttpContext HttpContext => GeneralContext.GetService<IHttpContextAccessor>()?.HttpContext;

        #endregion Public properties

        /// <summary> ctor </summary>
        public GeneralHttpClient(ApiServiceNames namedhttpClient, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _namedhttpClient = namedhttpClient;
            _logger = GeneralContext.GetService<ILogger>();
            RequestUrl = $"api/{_namedhttpClient}/";
        }

        #region Public methods

        /// <summary> Execute http request, path is relative or root, contentType json by default </summary>
        public TResult Execute<TResult>(HttpMethod httpMethod, string path, object data, string contentType = null)
        {
            var task = this.ExecuteAsync<TResult>(httpMethod, path, data, contentType);
            task.Wait();
            return task.Result;
        }

        /// <summary> CREATE and EXECUTE ASYNC Request with contentType, json by default </summary>
        public async Task<TResult> ExecuteAsync<TResult>(HttpMethod httpMethod, string path, object data, string contentType = null)
        {
            TResult result = default;
            HttpResponseMessage httpResponse;

            try
            {
                var dataArray = data as IEnumerable<object>;
                if (dataArray != null && dataArray.Count() == 0)
                    data = null;

                using (var httpRequest = CreateHttpRequest(httpMethod, path, data, contentType))
                using (var httpContent = CreareHttpContent(path, data, contentType))
                {
                    httpRequest.Content = httpContent;
                    httpResponse = await _httpClient.SendAsync(httpRequest);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.GetApiMessageInfo());
                //response.EnsureSuccessStatusCode();
                httpResponse = new HttpResponseMessage(HttpStatusCode.Conflict) { ReasonPhrase = ex.GetApiMessageInfo() };
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                ResponseDetails = CreateResponseDetails<TResult>(httpResponse, ex);
            }

            // SuccessStatusCode
            if (httpResponse != null)
                if (httpResponse.IsSuccessStatusCode)
                {
                    GeneralContext.Cache.CacheClearAll<TResult>(path);

                    result = await GetResponseResult<TResult>(httpResponse);
#if DEBUG
                    ResponseDetails = CreateResponseDetails<TResult>(httpResponse);
#endif
                }
                else
                {
                    if (ResponseDetails == null)
                        ResponseDetails = CreateResponseDetails<TResult>(httpResponse);
                    _logger.Error(ResponseDetails.ToString());
                }

            return result;
        }

        #endregion Public methods

        #region Privates methods

        private HttpContent CreareHttpContent(string path, object data, string contentType)
        {
            HttpContent httpContent;
            //TODO: CrpmZip
            if (contentType == "CustomMediaTypeNames.CrpmZip" || path.Contains("compress=true"))
            {
                data = Util.Compress(JsonConvert.SerializeObject(data));
                httpContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "Json");
            }
            else if (contentType == MediaTypeNames.Application.Octet)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms, Encoding.UTF8, 1024))
                    using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
                    {
                        var js = new JsonSerializer();
                        js.Serialize(jtw, data);
                        jtw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        httpContent = new StreamContent(ms);
                    }
                }
            }
            else
            {
                httpContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "Json");
            }

            return httpContent;
        }

        /// <summary> create http request /// </summary>
        private HttpRequestMessage CreateHttpRequest(HttpMethod httpMethod, string path, object data, string contentType = null)
        {
            //create httpRequestMessage
            RequestUrl = _httpClient.BaseAddress.ToString() + UrlFormat(path);
            //TODO: CrpmZip
            contentType = contentType == "CustomMediaTypeNames.CrpmZip" ? "Json" : contentType;

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(RequestUrl),
                Headers = { { HttpRequestHeader.ContentType.ToString(), contentType ?? "Json" } },
                Content = data != null ? new StringContent(JsonConvert.SerializeObject(data)) : null
            };

            return httpRequestMessage;
        }

        /// <summary> Get response result /// </summary>
        private async Task<TResult> GetResponseResult<TResult>(HttpResponseMessage httpResponse)
        {
            TResult result;

            // HttpResponseMessage
            if (typeof(TResult) == typeof(HttpResponseMessage))
            {
                result = (TResult)Convert.ChangeType(httpResponse, typeof(TResult));
            }
            // FileContentResult
            else if (typeof(TResult) == typeof(FileContentResult))
            {
                var streamResult = await httpResponse.Content.ReadAsByteArrayAsync();
                var fileName = httpResponse.Content.Headers.ContentDisposition.FileName;
                var content = new FileContentResult(streamResult.ToArray(), MediaTypeNames.Application.Octet) { FileDownloadName = fileName };
                result = (TResult)Convert.ChangeType(content, typeof(TResult));
            }
            // FileStreamResult
            else if (typeof(TResult) == typeof(FileStreamResult))
            {
                var streamResult = await httpResponse.Content.ReadAsStreamAsync();
                var fileName = httpResponse.Content.Headers.ContentDisposition.FileName;
                var content = new FileStreamResult(streamResult, MediaTypeNames.Application.Octet) { FileDownloadName = fileName };
                result = (TResult)Convert.ChangeType(content, typeof(TResult));
            }
            // MemoryStream
            else if (typeof(TResult) == typeof(MemoryStream))
            {
                Stream stream = await httpResponse.Content.ReadAsStreamAsync();
                result = (TResult)Convert.ChangeType(stream, typeof(TResult));
            }
            // Json by default
            else
            {
                result = GetResponseJsonResult<TResult>(httpResponse);
            }
            return result;
        }

        /// <summary> Get response json result /// </summary>
        private TResult GetResponseJsonResult<TResult>(HttpResponseMessage response)
        {
            TResult result = default;
            try
            {
                string[] simpleTypes = { "int32", "single", "double", "string", "boolean" };
                var stringResult = response.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(stringResult))
                {
                    result = default;
                }
                else if (simpleTypes.Contains(typeof(TResult).Name.ToLower()))
                //else if (typeof(TResult).IsValueType || typeof(TResult) == typeof(string))
                {
                    result = (TResult)Convert.ChangeType(stringResult, typeof(TResult));
                }
                else if (typeof(TResult) == typeof(ObjectResult) || typeof(TResult) == typeof(ActionResult<>))
                {
                    result = (TResult)Convert.ChangeType(
                      new ObjectResult(stringResult) { StatusCode = (int?)response.StatusCode }, typeof(TResult));
                }
                else
                {
                    result = JsonConvert.DeserializeObject<TResult>(stringResult);
                }

                if (EqualityComparer<TResult>.Default.Equals(result, default))
                    _logger.Warning($"result of type {typeof(TResult)} is equals to default value: {result}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.GetApiMessageInfo());
            }
            return result;
        }

        /// <summary> Create response details /// </summary>
        private ApiResponse CreateResponseDetails<TResult>(HttpResponseMessage response, Exception apiException = null, string message = null)
        {
            ApiResponse apiResponse = null;
            try
            {
                // get action  declaredType
                Type responseDeclaredType = typeof(TResult); //HttpContext.GetActionReturnType();

                HttpStatusCode statusCode = apiException != null ? HttpStatusCode.Conflict : response.StatusCode;

                string messageInfo = !response.IsSuccessStatusCode
                    ? response.Content?.ReadAsStringAsync().Result
                    : response.StatusCode.ToString();

                // create api response
                apiResponse = new ApiResponse((int)response.StatusCode)
                {
                    TraceId = GeneralContext.CreateTraceId(),
                    RequestUrl = GeneralContext.HttpContext?.GetRequestUrl(),
                    Value = responseDeclaredType?.GetDefault(),
                    ActionType = responseDeclaredType,
                    Message = message ?? apiException?.GetApiMessageInfo() ?? messageInfo,
                    Error = apiException != null ? new ApiError(apiException) : null,
                    StatusCode = $"{(int)statusCode}: {statusCode}"
                };

                _logger.Information($"traceId: {apiResponse.TraceId}, message: {apiResponse.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.GetApiMessageInfo("Unhandled exception"));
                //context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }

            return apiResponse;
        }

        /// <summary> format path URL depend of data and http method. /// </summary>
        private string UrlFormat(string path)
        {
            if (!path.StartsWith("/") && !path.Contains("api/"))
                path = $"api/{this.HttpClientName}/{path}";

            //replace '//' to '/'
            if (path.StartsWith("/"))
                path = path.Substring(1);

            return path;
        }

        private void ToStream(object value, Stream stream)
        {
            using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                var js = new JsonSerializer();
                js.Serialize(jtw, value);
                jtw.Flush();
            }
        }

        public void Dispose()
        {
            ((IDisposable)_httpClient).Dispose();
        }

        #endregion
    }
}
