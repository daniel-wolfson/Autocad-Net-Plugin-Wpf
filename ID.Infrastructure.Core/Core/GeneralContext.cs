using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace ID.Infrastructure.Core
{
    /// <summary> General context, contains: Identity, ServiceProvider, HttpContext, Cache, Configurations, Helpers </summary>
    public static class GeneralContext
    {
        #region ServiceProvider

        public static IServiceProvider ServiceProvider { get; set; }
        public static IServiceScope ServiceScope { get; set; }

        public static IServiceScope CreateServiceScope()
        {
            return ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }

        public static TService GetService<TService>()
        {
            TService serviceResult = default;

            try
            {
                if (ServiceScope != null)
                    serviceResult = (TService)HttpContext.RequestServices.GetService(typeof(TService));
                else
                    serviceResult = (TService)ServiceProvider.GetService(typeof(TService));

                if (serviceResult == null)
                    throw new NotImplementedException("service not implemented");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, ex.GetApiMessageInfo());
            }

            return serviceResult;
        }

        public static object GetService(Type serviceType)
        {
            object serviceResult = null;

            try
            {
                if (ServiceScope != null)
                    serviceResult = HttpContext.RequestServices.GetService(serviceType);
                else
                    serviceResult = ServiceProvider.GetService(serviceType);

                if (serviceResult == null)
                    throw new NotImplementedException("service not implemented");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, ex.GetApiMessageInfo());
            }

            return serviceResult;
        }

        public static void SetServiceProvider(IServiceProvider services)
        {
            ServiceProvider = services;
            Cache = new MemoryCache(new MemoryCacheOptions());

            ServicePointManager.ServerCertificateValidationCallback +=
                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true; // **** Always accept
                };
        }

        #endregion ServiceProvider

        #region HttpContext & HttpCliens

        // TraceIdentifier preventing calling twice
        public static string RequestTraceId { get; set; }

        public static GeneralHttpClient CreateRestClient(ApiServiceNames httpClientNamed = ApiServiceNames.DalApi)
        {
            IHttpClientFactory httpContextFactory = GetService<IHttpClientFactory>();
            var typedHttpClient = httpContextFactory.CreateClient(httpClientNamed.GetDisplayName());
            return new GeneralHttpClient(httpClientNamed, typedHttpClient);
        }

        /// <summary> Provides static access to the current HttpContext /// </summary>
        private static HttpContext _httpContext;
        public static HttpContext HttpContext
        {
            get
            {
                return _httpContext ?? ServiceProvider.GetService<IHttpContextAccessor>().HttpContext;
            }
            set
            {
                _httpContext = value;
            }
        }

        public static string CreateTraceId()
        {
            return Guid.NewGuid().ToString("n").Substring(0, 8);
        }

        #endregion HttpContext & HttpCliens

        #region Cache

        private static IEnumerable<string> _controllers;

        // Keep in cache for this time, reset time if accessed.
        public static MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10));

        public static IMemoryCache Cache { get; set; }

        public static Dictionary<string, object> CacheResults
        {
            get
            {
                var field = Cache.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                var collection = field.GetValue(Cache) as ICollection;
                var items = new Dictionary<string, object>();

                if (collection == null) return new Dictionary<string, object>();


                foreach (var cacheItem in collection)
                {
                    var methodInfo = cacheItem.GetType().GetProperty("Key");
                    var value = cacheItem.GetType().GetProperty("Value").GetValue(cacheItem, null);
                    value = value.GetType().GetProperty("Value").GetValue(value, null);
                    var k = methodInfo.GetValue(cacheItem).ToString();
                    items.Add(k, value);
                }

                return items;
            }
        }

        public static IEnumerable<string> ControllerNames
        {
            get
            {
                if (_controllers == null)
                {
                    _controllers = Assembly.GetEntryAssembly()?.GetTypes()
                        .Where(type => typeof(Controller).IsAssignableFrom(type))
                        .Select(x => x.Name.Replace("Controller", ""));
                }
                return _controllers;
            }
        }

        #endregion

        #region Configurations

        public static ILogger Logger => ServiceProvider.GetService<ILogger>();

        /// <summary> get config from app settings and cast to the startup's registred concrete class </summary>
        public static TConfig GetConfig<TConfig>()
        {
            var appConfig = GetService<TConfig>();
            return appConfig;
        }

        public static string GetConnectionString(string sectionName)
        {
            IConfiguration config = GetService<IConfiguration>();
            string connectionString = config.GetConnectionString(sectionName);
            return connectionString;
        }

        #endregion Configurations

        #region Helpers 

        private static readonly Lazy<List<object>> _LastErrors = new Lazy<List<object>>(() => new List<object>());
        /// <summary> Get error/exception list </summary>
        public static List<object> LastErrors => _LastErrors.Value;

        /// <summary> Get api message info for logging </summary>
        public static string GetApiMessageInfo(string message, EventLevel eventLevel, params object[] parameters)
        {
            var _parameters = parameters != null && parameters.Length > 0
                    ? $"\nParameters:{ConvertParametersToMessage(parameters)} " : "";

            return $"{Assembly.GetEntryAssembly().GetName().Name}, Level: {eventLevel}, Message: {message}{_parameters}";
        }

        /// <summary> Convert oarameters to message for logging </summary>
        public static string ConvertParametersToMessage(params object[] parameters)
        {
            string msg = string.Empty;
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    IEnumerable collection = parameters[0] as IEnumerable;
                    if (collection != null)
                    {
                        foreach (var item in collection)
                        {
                            msg += item.ToString() + " ";
                        }
                    }
                    else
                    {
                        msg += parameters[0].ToString() + " ";
                    }
                }
            }
            return msg;
        }

        /// <summary> General localizer </summary>
        public static IStringLocalizer Localizer => GetService<IStringLocalizer>();

        /// <summary> Global language </summary>
        public static string Language { get; set; } = "en";

        #endregion Helpers
    }
}
