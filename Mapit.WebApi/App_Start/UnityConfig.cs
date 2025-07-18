using AspNet.Identity.PostgreSQL;
using ID.Infrastructure;
using ID.Infrastructure.Models;
using Intellidesk.Data.Auth;
using Intellidesk.Data.Services;
using System;
using System.Web.Hosting;
using System.Web.Http;
using Unity;
using Unity.Lifetime;

namespace MapIt.WebApi
{
    /// <summary> Specifies the Unity configuration for the main container. /// </summary>
    public static class UnityConfig
    {
        private static readonly Lazy<IUnityContainer> _container = new Lazy<IUnityContainer>(() =>
         {
             var container = new UnityContainer();
             RegisterTypes(container);
             return container;
         });

        /// <summary> Configured Unity Container. /// </summary>
        public static IUnityContainer Container => _container.Value;

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();

            var settings = PluginSettings.Build(HostingEnvironment.MapPath(@"~/App_Data"));
            container.RegisterInstance(settings, new ContainerControlledLifetimeManager());
            container.RegisterInstance(Plugin.Logger, new ContainerControlledLifetimeManager());

            container.RegisterType<IAdminService<IdentityUserDetails>, UserService>(new ContainerControlledLifetimeManager());
            container.RegisterType<AuthRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<AppUserStore>(new ContainerControlledLifetimeManager());
            container.RegisterType<AppDbContext>(new ContainerControlledLifetimeManager());

            //GlobalConfiguration.Configuration.Services.Add(typeof(ModelValidatorProvider), new CustomModelValidatorProvider());
            GlobalConfiguration.Configuration.DependencyResolver = new UnityResolver(container);
        }

        public static void Register(HttpConfiguration config)
        {
            var container = new UnityContainer();
            RegisterTypes(container);

            config.DependencyResolver = new UnityResolver(container);
        }

        public static void Register(HttpConfiguration config, IUnityContainer container)
        {
            RegisterTypes(container);
            config.DependencyResolver = new UnityResolver(container);
        }
    }
}