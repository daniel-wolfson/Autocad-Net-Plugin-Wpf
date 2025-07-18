using ID.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ID.Infrastructure.Extensions
{
    public static class HttpClientExtensions
    {
        /// <summary> add header from current http request </summary>
        public static void AddFromRequest(this HttpHeaders httpHeaders, string headerName)
        {
            //IHttpContextAccessor accessor = GeneralContext.GetService<IHttpContextAccessor>();
            //AppUser appUser = accessor?.HttpContext.Items["loggedUser"] as AppUser;
            ////bool existUserAuthenticated = accessor?.HttpContext?.User?.Identities.Any(x => x.IsAuthenticated) ?? false;
            //if (appUser != null)
            //{
            //    //ClaimsPrincipal identityUser = accessor.HttpContext.User;
            //    //var claim = accessor.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.UserData.ToString());
            //    //var userJson = Util.DecryptText(claim.Value, authOptions.KEY);
            //    //IAppUser loggedUser = JsonConvert.DeserializeObject<AppUser>(userJson);

            //    string headerValue = accessor.HttpContext.Request.Headers[headerName];
            //    if (!string.IsNullOrEmpty(headerValue))
            //        httpHeaders.Add(headerName, headerValue);
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

        /// <summary> send GET request to api and returning ObjectResult with status and value, path is relative or root</summary>
        public static TResult GetResult<TResult>(this GeneralHttpClient apiClient, string path)
        {
            var cacheKey = path + "_" + typeof(TResult).ToString();
            if (GeneralContext.Cache.TryGetValue(cacheKey, out TResult cacheResult))
                return cacheResult;

            var result = apiClient.Get<TResult>(path);

            GeneralContext.Cache.Set(cacheKey, result, GeneralContext.CacheEntryOptions);

            return result;
        }

        /// <summary> GET ASYNC request to api and returning ObjectResult with status and value, path is relative or root</summary>
        public static Task<TResult> GetResultAsync<TResult>(this GeneralHttpClient apiClient, string path)
        {
            var cacheKey = GeneralContext.Cache.CacheCreateKey<TResult>(path);
            if (GeneralContext.Cache.TryGetValue(cacheKey, out TResult cacheResult))
                return Task.FromResult(cacheResult);

            var result = apiClient.GetAsync<TResult>(path).ContinueWith(t =>
            {
                GeneralContext.Cache.Set(cacheKey, t.Result, GeneralContext.CacheEntryOptions);
                return t.Result;
            });

            return result;
        }

        /// <summary> PATCH request to api with SaveChanges, path is relative or root</summary>
        public static TResult Update<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.Execute<TResult>(HttpMethod.Put, path, data, contentType);
        }

        /// <summary> PATCH ASYNC request to api with SaveChanges, path is relative or root</summary>
        public static Task<TResult> UpdateAsync<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = null)
        {
            return apiClient.ExecuteAsync<TResult>(HttpMethod.Put, path, data, contentType);
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
        public static async Task<TResult> PostCompressAsync<TResult>(this GeneralHttpClient apiClient, string path, object data, string contentType = CustomMediaTypeNames.CrpmZip)
        {
            var result = await apiClient.ExecuteAsync<TResult>(HttpMethod.Post, path, data, contentType);
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
                    var isCustomTimeout = double.TryParse(appConfig.ResponseTimeout, out double apiResponseTimeout);
                    httpClient.Timeout = TimeSpan.FromMinutes(isCustomTimeout ? apiResponseTimeout : 10);
                })
                .ConfigurePrimaryHttpMessageHandler(provider =>
                {
                    var handler = new HttpClientHandler();
                    var env = provider.GetService<IHostEnvironment>();
                    if (env.IsDevelopment())
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    return handler;
                });
        }
    }
}
