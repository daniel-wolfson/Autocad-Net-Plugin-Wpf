using ID.Infrastructure.Core;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace ID.Infrastructure.Extensions
{
    public static class HttpClientExtensions
    {
        /// <summary> add header from current http request </summary>
        public static void AddFromRequest(this HttpHeaders httpHeaders, string headerName)
        {
            IHttpContextAccessor accessor = GeneralContext.GetService<IHttpContextAccessor>();
            AppUser appUser = accessor?.HttpContext.Items["loggedUser"] as AppUser;
            //bool existUserAuthenticated = accessor?.HttpContext?.User?.Identities.Any(x => x.IsAuthenticated) ?? false;
            if (appUser != null)
            {
                //ClaimsPrincipal identityUser = accessor.HttpContext.User;
                //var claim = accessor.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData.ToString());
                //var userJson = Util.DecryptText(claim.Value, authOptions.KEY);
                //IAppUser loggedUser = JsonConvert.DeserializeObject<AppUser>(userJson);

                string headerValue = accessor.HttpContext.Request.Headers[headerName];
                if (!string.IsNullOrEmpty(headerValue))
                    httpHeaders.Add(headerName, headerValue);
            }
        }

        #region CRUD methods

        /// <summary> GET request to api, path is relative or root</summary>
        public static TResult Get<TResult>(this GeneralHttpClient apiClient, string path)
        {
            return apiClient.Execute<TResult>(HttpMethod.Get, path, null);
        }

        /// <summary> GET ASYNC request to api, path is relative or root, path is relative or root</summary>
        public static Task<TResult> GetAsync<TResult>(this GeneralHttpClient apiClient, string path)
        {

            return apiClient.ExecuteAsync<TResult>(HttpMethod.Get, path, null);
        }

        /// <summary> PATCH request to api with SaveChanges, path is relative or root</summary>
        public static TResult Update<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.Execute<TResult>(HttpMethod.Patch, path, data, contentType);
        }

        /// <summary> PATCH ASYNC request to api with SaveChanges, path is relative or root</summary>
        public static Task<TResult> UpdateAsync<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.ExecuteAsync<TResult>(HttpMethod.Patch, path, data, contentType);
        }

        /// <summary> PUT request to api with SaveChanges, path is relative or root</summary>
        public static TResult Add<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.Execute<TResult>(HttpMethod.Put, path, data, contentType);
        }


        /// <summary> PUT ASYNC request to api with SaveChanges, path is relative or root</summary>
        public static Task<TResult> AddAsync<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.ExecuteAsync<TResult>(HttpMethod.Put, path, data, contentType);
        }

        /// <summary> DELETE request to api with SaveChanges, path is relative or root</summary>
        public static TResult Delete<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.Execute<TResult>(HttpMethod.Delete, path, data, contentType);
        }


        /// <summary> DELETE ASYNC request to api with SaveChanges, path is relative or root</summary>
        public static Task<TResult> DeleteAsync<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.ExecuteAsync<TResult>(HttpMethod.Delete, path, data, contentType);
        }

        /// <summary> POST request to api, path is relative or root</summary>
        public static TResult Post<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.Execute<TResult>(HttpMethod.Post, path, data, contentType);
        }

        /// <summary> ASYNC POST request to api, path is relative or root</summary>
        public static Task<TResult> PostAsync<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.ExecuteAsync<TResult>(HttpMethod.Post, path, data, contentType);
        }

        /// <summary> ASYNC COMPRESS POST request to api, path is relative or root</summary>
        public static async Task<TResult> PostCompressAsync<TResult>(this GeneralHttpClient apiClient, string path, object data)
        {
            var result = await apiClient.ExecuteAsync<TResult>(HttpMethod.Post, path, data, CustomMediaTypeNames.CrpmZip);
            return result;
        }

        public static async Task<TResult> PostStreamAsync<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = CustomMediaTypeNames.FormData)
        {
            IDictionary<string, object> dataArray;
            if (data is Dictionary<string, object>)
                dataArray = data as Dictionary<string, object>;
            else
                dataArray = data.ToDictionary();

            HttpContent httpContent;
            switch (contentType)
            {
                case CustomMediaTypeNames.FormDataCompress:
                case CustomMediaTypeNames.FormData:
                    httpContent = new MultipartFormDataContent();
                    foreach (var dataItem in dataArray)
                    {
                        byte[] byteArrary;
                        if (contentType == CustomMediaTypeNames.FormDataCompress)
                            byteArrary = Util.Compress(Util.Compress(Util.ConvertObjectToByteArray(dataItem.Value)));
                        else
                            byteArrary = Util.ConvertObjectToByteArray(dataItem.Value);

                        ByteArrayContent byteArrayContent = new ByteArrayContent(byteArrary);
                        byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(CustomMediaTypeNames.OctetStream);
                        ((MultipartFormDataContent)httpContent).Add(byteArrayContent, dataItem.Key, dataItem.Key);

                        if (contentType == CustomMediaTypeNames.FormDataCompress)
                            httpContent.Headers.Add("Compress", "true");
                    }
                    break;
                default:
                    httpContent = new StringContent("no content");
                    break;
            }

            var result = await apiClient.ExecuteAsync<TResult>(HttpMethod.Post, path, httpContent, contentType);
            return result;
        }

        #endregion CRUD methods

        public static void AddHttpClient(this IServiceCollection services, string namedClient, IConfiguration config)
        {
            var endpoint = config.GetSection($"AppConfig:Endpoints:{namedClient}").Value;
            var appConfig = config.GetSection("AppConfig").Get<AppConfig>();

            services.AddHttpClient<GeneralHttpClient>(namedClient,
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(endpoint);
                    httpClient.DefaultRequestHeaders.Add("User-Agent", appConfig.Domain);
                    httpClient.DefaultRequestHeaders.Add("X-Named-Client", namedClient);
                    httpClient.DefaultRequestHeaders.AddFromRequest("Authorization");
                    httpClient.DefaultRequestHeaders.AddFromRequest("Accept-Language");
                    var isCustomTimeout = double.TryParse(appConfig.ResponseTimeout, out double apiResponseTimeout);
                    httpClient.Timeout = TimeSpan.FromMinutes(isCustomTimeout ? apiResponseTimeout : 10);
                })
                .ConfigurePrimaryHttpMessageHandler(provider =>
                {
                    var handler = new HttpClientHandler();
                    var env = provider.GetService<IWebHostEnvironment>();
                    if (env.IsDevelopment())
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    return handler;
                });
        }

        public static IDictionary<string, object> ToDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
}
