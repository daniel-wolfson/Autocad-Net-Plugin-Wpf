using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Unity;

namespace ID.Infrastructure
{
    public static class GeneralContext
    {
        #region Identity

        private static IAppUser _loggedUser = null;
        public static IAppUser LoggedUser
        {
            get
            {
                var appUser = HttpContext.Items["loggedUser"] as AppUser;
                //IHttpContextAccessor accessor = (IHttpContextAccessor)ServiceProvider.GetService(typeof(IHttpContextAccessor));
                _loggedUser = HttpContext.User.ToAppUser<AppUser>() as AppUser;
                return _loggedUser;
            }
            set
            {
                _loggedUser = value;
            }
        }

        #endregion Identity

        #region ServiceProvider

        public static IServiceProvider ServiceProvider { get; set; }
        public static IServiceScope ServiceScope { get; set; }

        public static IServiceScope CreateServiceScope()
        {
            return GeneralContext.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }

        public static TService GetService<TService>()
        {
            TService serviceResult = default;

            try
            {
                if (ServiceScope != null)
                    serviceResult = default; //TODO!!!: (TService)HttpContext.RequestServices.GetService(typeof(TService));
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
            object serviceResult;
            var tservice = ServiceProvider.GetService(serviceType);
            if (tservice != null)
            {
                serviceResult = tservice;
            }
            else
            {
                var ex = new NotImplementedException("service not implemented");
                Logger.Fatal(ex, $"service {serviceType.Name} not registred");
                throw ex;
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
                return null; //_httpContext ?? ServiceProvider.GetService<IHttpContextAccessor>().HttpContext;
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

        #endregion Helpers
    }

    public static class GeneralContext1
    {
        private static readonly Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            return new UnityResolver(new UnityContainer()).GetContainer();
        });
        public static IUnityContainer Container = container.Value;

        public static string RequestTraceId { get; set; }

        /// <summary> Provides static access to the framework's services provider </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        private static AppUser _loggedUser = null;
        public static AppUser LoggedUser
        {
            get
            {
                //IHttpContextAccessor accessor = (IHttpContextAccessor)ServiceProvider.GetService(typeof(IHttpContextAccessor));
                //if (_loggedUser != null && accessor.HttpContext.TraceIdentifier == GeneralContext.RequestTraceId)
                //    return _loggedUser;
                //_loggedUser = accessor.HttpContext.User.ToAppUser<AppUser>() as AppUser;
                //_loggedUser = HttpContext.User.ToAppUser<AppUser>() as AppUser;
                return _loggedUser;
            }
            set
            {
                _loggedUser = value;
            }
        }

        //public static HttpContext HttpContext => GetService<IHttpContextAccessor>()?.HttpContext;

        /// <summary> Get registred service </summary>
        public static TService GetService<TService>() //where T : class
        {
            //if (typeof(TService) == typeof(IDBContext))
            //    service = Container.Resolve<TService>();
            //else
            TService service = (TService)ServiceProvider.GetService(typeof(TService));
            if (service == null)
            {
                var ex = new NotImplementedException("service not implemented");
                Log.Logger.ErrorCall(ex);
                throw ex;
            }
            return service;
        }

        /// <summary> Init services provider </summary>
        public static void AddServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary> Init services provider </summary>
        public static void AddServiceProvider(IUnityContainer container)
        {
            Container = container;
        }

        public static object ResolveService<TService>() where TService : class
        {
            return Container.Resolve<TService>();
        }

        public static object BuildUp<TService>(TService obj, Func<IUnityContainer, TService> implementationFactory) where TService : class
        {
            return Container.BuildUp(obj);
        }

        //public static IEnumerable<IAppPermissionsInfo> GetRolePermissionsTabs()
        //{
        //    var permissionTabs = GetClaim<IEnumerable<RolePermissionsTabsInfoExt>>(ClaimTypesExt.PermissionTabs);
        //    if (permissionTabs != null)
        //        return permissionTabs.Cast<IAppPermissionsInfo>();
        //    else
        //        return null;
        //}

        public static IEnumerable<IAppRole> GetClaimRoles()
        {
            var results = new List<UserOrganizationsRolesExt>();
            //var httpContext = GetService<IHttpContextAccessor>().HttpContext;
            //ClaimsPrincipal currentUser = httpContext.User;

            //var value = currentUser.Claims.FirstOrDefault(x => x.Type == ClaimTypesExt.RoleData);
            //if (value != null)
            //{
            //    results.Add(JsonConvert.DeserializeObject<UserOrganizationsRolesExt>(value.Value));
            //}
            //else
            //{
            //    var roleData = httpContext.Session.GetString(SessionKeys.RoleData.ToString());
            //    if (!string.IsNullOrEmpty(roleData)) {
            //        results.Add(JsonConvert.DeserializeObject<UserOrganizationsRole>(roleData));
            //    }
            //}

            return results.Cast<IAppRole>();
        }

        public static T GetClaim<T>(string claimName)
        {
            T result = default;
            //var httpContext = GetService<IHttpContextAccessor>().HttpContext;
            //ClaimsPrincipal currentUser = httpContext.User;

            //var value = currentUser.FindFirst(claimName);

            //if (claimName == "SelectedOrganizationName")
            //    value = null;

            //if (value != null)
            //{
            //    if (typeof(T) == typeof(Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>))
            //        result = (T)Convert.ChangeType(JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>>(value.Value), typeof(Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>));
            //    if (typeof(T) == typeof(string))
            //        result = (T)Convert.ChangeType(value.Value, typeof(string));
            //    else
            //        result = JsonConvert.DeserializeObject<T>(value.Value);
            //}

            return result;
        }

        //public static string GetSessionData(IAppConfig config)
        //{
        //    IHttpContextAccessor httpContextAccessor = GetService<IHttpContextAccessor>();
        //    Dictionary<string, string> sessionItems = new Dictionary<string, string>
        //    {
        //        { SessionKeys.ClientName.GetDisplayName(), config.ClientName },
        //        { SessionKeys.DbName.GetDisplayName(), config.DalDb },
        //        { SessionKeys.SelectedCultureName.GetDisplayName(), config.DefaultCultureName  },
        //        { SessionKeys.SelectedOrganizationName.GetDisplayName(),
        //            (httpContextAccessor.HttpContext.Session.Get(SessionKeys.SelectedOrganizationName) ?? "").ToBase64String() }
        //    };

        //    sessionItems.Add(SessionKeys.SelectedOrganizationId.GetDisplayName(), httpContextAccessor.HttpContext.Session.Contains(SessionKeys.SelectedOrganizationId)
        //            ? httpContextAccessor.HttpContext.Session.Get<int>(SessionKeys.SelectedOrganizationId).ToString() : "");
        //    return JsonConvert.SerializeObject(sessionItems);
        //}

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                   SingleOrDefault(assembly => assembly.GetName().Name == name);
        }

        public static void SetServiceProvider(Microsoft.Extensions.DependencyInjection.ServiceProvider sp)
        {
            ServiceProvider = sp;
        }
    }

}
