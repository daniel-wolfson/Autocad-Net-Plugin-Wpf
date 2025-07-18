using General.Infrastructure.Interfaces;
using General.Infrastructure.Models;
using Intellidesk.Common.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using Unity;

namespace General.Infrastructure.Core
{
    public static class GeneralContext
    {
        public static string RequestTraceId { get; set; }

        /// <summary> Provides static access to the framework's services provider </summary>
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IUnityContainer Container { get; private set; }

        private static AppUser _loggedUser = null;
        public static AppUser LoggedUser
        {
            get
            {
                //IHttpContextAccessor accessor = (IHttpContextAccessor)ServiceProvider.GetService(typeof(IHttpContextAccessor));
                //_loggedUser = accessor.HttpContext.User.ToAppUser<AppUser>() as AppUser;
                return _loggedUser;
            }
            set
            {
                _loggedUser = value;
            }
        }

        /// <summary> Get registred service </summary>
        public static TService GetService<TService>() //where T : class
        {
            TService service = default;

            //if (typeof(TService) == typeof(IDBContext))
            //    service = Container.Resolve<TService>();
            //else
            service = (TService)ServiceProvider.GetService(typeof(TService));

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
            return null;
        }
        //public static IEnumerable<IUserOrganizationsRole> GetClaimRoles()
        //{
        //    var results = new List<UserOrganizationsRolesExt>();
        //    var httpContext = GetService<IHttpContextAccessor>().HttpContext;
        //    ClaimsPrincipal currentUser = httpContext.User;

        //    var value = currentUser.Claims.FirstOrDefault(x => x.Type == ClaimTypesExt.RoleData);
        //    if (value != null)
        //    {
        //        results.Add(JsonConvert.DeserializeObject<UserOrganizationsRolesExt>(value.Value));
        //    }
        //    //else
        //    //{
        //    //    var roleData = httpContext.Session.GetString(SessionKeys.RoleData.ToString());
        //    //    if (!string.IsNullOrEmpty(roleData)) {
        //    //        results.Add(JsonConvert.DeserializeObject<UserOrganizationsRole>(roleData));
        //    //    }
        //    //}

        //    return results.Cast<IUserOrganizationsRole>();
        //}

        //public static T GetClaim<T>(string claimName)
        //{
        //    T result = default;
        //    var httpContext = GetService<IHttpContextAccessor>().HttpContext;
        //    ClaimsPrincipal currentUser = httpContext.User;

        //    var value = currentUser.FindFirst(claimName);

        //    if (claimName == "SelectedOrganizationName")
        //        value = null;

        //    if (value != null)
        //    {
        //        if (typeof(T) == typeof(Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>))
        //            result = (T)Convert.ChangeType(JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>>(value.Value), typeof(Dictionary<string, IEnumerable<RolePermissionsTabsInfoExt>>));
        //        if (typeof(T) == typeof(string))
        //            result = (T)Convert.ChangeType(value.Value, typeof(string));
        //        else
        //            result = JsonConvert.DeserializeObject<T>(value.Value);
        //    }

        //    return result;
        //}

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

        //public static Assembly GetAssemblyByName(string name)
        //{
        //    return AppDomain.CurrentDomain.GetAssemblies().
        //           SingleOrDefault(assembly => assembly.GetName().Name == name);
        //}
    }

}
