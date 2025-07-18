using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ID.Infrastructure.Core
{
    /// <summary> General named rest http client </summary>
    public class GeneralHttpClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ApiServiceNames _namedhttpClient;
        private readonly ILogger _logger;
        private TransactionScope _transactionScope;

        public ApiHttpHeaders ApiHttpHeaders = new ApiHttpHeaders();

        #region Public properties

        public string HttpClientName => _namedhttpClient.ToString();
        public Uri BaseAddress => _httpClient.BaseAddress;
        public string RequestUrl { get; private set; }
        public ApiResponse ResponseDetails { get; private set; }
        public long MaxResponseContentBufferSize => _httpClient.MaxResponseContentBufferSize;
        public TimeSpan Timeout => _httpClient.Timeout;

        //public HttpContext HttpContext => GeneralContext.GetService<IHttpContextAccessor>()?.HttpContext;
        /// <summary> Provides static access to the current HttpContext /// </summary>
        private HttpContext _httpContext;
        public HttpContext HttpContext
        {
            get
            {
                if (_httpContext == null)
                    _httpContext = GeneralContext.ServiceProvider.GetService<IHttpContextAccessor>().HttpContext;
                return _httpContext;
            }
            set { _httpContext = value; }
        }

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
                using (var httpRequest = CreateHttpRequest(httpMethod, path, data, contentType))
                {
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

            _httpContext = null;
            return result;
        }

        /// <summary> Initializes a new transaction scope for long operations </summary>
        public TransactionScope CreateTransaction([CallerFilePath] string callerPath = "", [CallerMemberName] string callerName = "")
        {
            //var controllerType = new StackFrame(1, false).GetMethod().DeclaringType.ReflectedType;
            //MethodInfo methodInfo = controllerType.GetMethod(callerName);
            return _transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }

        public void AddRequestHeader(string headerName, string headerValue)
        {
            if (!ApiHttpHeaders.Contains(headerName))
                ApiHttpHeaders.Add(headerName, headerValue);
        }

        public void RemoveRequestHeader(string headerName)
        {
            if (ApiHttpHeaders.Contains(headerName))
                ApiHttpHeaders.Remove(headerName);
        }

        #endregion Public methods

        #region Privates methods

        private HttpRequestMessage CreateHttpRequest(HttpMethod httpMethod, string path, object data, string contentType)
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(_httpClient.BaseAddress.ToString() + UrlFormat(path)),
                Headers = { { HttpRequestHeader.ContentType.ToString(), contentType ?? MediaTypeNames.Application.Json } },
                Content = data != null ? CreateHttpContent(path, data, contentType) : null
            };

            if (HttpContext.Items.ContainsKey("ApiTransactionToken")) //if (Transaction.Current != null) Transaction.Current.TransactionInformation.LocalIdentifier
            {
                var token = Encoding.UTF8.GetBytes($"{this.HttpContext.Items["ApiTransactionToken"]}");
                httpRequest.Headers.Add("ApiTransactionToken", Convert.ToBase64String(token));
            }

            if (ApiHttpHeaders.Any())
                foreach (var httpHeader in ApiHttpHeaders)
                {
                    if (!httpRequest.Headers.Contains(httpHeader.Key))
                        httpRequest.Headers.Add(httpHeader.Key, httpHeader.Value);
                }

            return httpRequest;
        }

        /// <summary> create http request /// </summary>
        private HttpContent CreateHttpContent(string path, object data, string contentType)
        {
            HttpContent httpContent;

            if (contentType == CustomMediaTypeNames.CrpmZip || path.Contains("compress=true"))
            {
                data = Util.Compress(JsonConvert.SerializeObject(data));
                httpContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, MediaTypeNames.Application.Json);
            }
            else if (contentType == CustomMediaTypeNames.FormData || contentType == CustomMediaTypeNames.FormDataCompress || contentType == CustomMediaTypeNames.XWwwFormUrlencoded)
            {
                httpContent = data as MultipartFormDataContent;
            }
            else if (contentType == CustomMediaTypeNames.ByteArrayContent)
            {
                httpContent = data as ByteArrayContent;
                httpContent.Headers.Clear();
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(CustomMediaTypeNames.OctetStream);
            }
            else if (contentType == CustomMediaTypeNames.OctetStream)
            {
                httpContent = new ByteArrayContent(Util.ConvertObjectToByteArray(data));
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(CustomMediaTypeNames.OctetStream);
            }
            else if (contentType == CustomMediaTypeNames.ContentStream)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms, Encoding.UTF8, 1024);
                    var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None };
                    {
                        var js = new JsonSerializer();
                        js.Serialize(jtw, data);
                        jtw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        httpContent = new StreamContent(ms);
                    }
                }
                httpContent.Headers.Add("Content-Type", MediaTypeNames.Application.Octet);
            }
            else if (contentType == CustomMediaTypeNames.Bson)
            {
                MemoryStream ms = new MemoryStream();
                using (BsonDataWriter writer = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, data);
                }

                string dataString = Convert.ToBase64String(ms.ToArray());
                httpContent = new StringContent(dataString, Encoding.UTF8, MediaTypeNames.Application.Json);
            }
            else
            {
                httpContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, MediaTypeNames.Application.Json);
            }

            return httpContent;
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

        public void Dispose()
        {
            ((IDisposable)_httpClient).Dispose();
            _transactionScope?.Complete();
            _transactionScope?.Dispose();
        }

        #endregion
    }
}
