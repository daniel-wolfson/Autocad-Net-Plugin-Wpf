using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.Owin.Hosting;
using Owin;
using Prism.Ioc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Windows.Threading;
using Unity;
using Unity.Lifetime;

//[assembly: OwinStartup(typeof(Plugin))]
namespace ID.Infrastructure
{
    public class Plugin : IRegisterModule
    {
        #region <props>

        public static Dispatcher BackgroundDispatcher { get; set; }

        private static readonly Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
                {
                    //UnityResolver globalUnityResolver = GlobalConfiguration.Configuration.DependencyResolver as UnityResolver;
                    //if (globalUnityResolver != null && globalUnityResolver.GetContainer().Registrations.Any())
                    //    container.AddExtension(new MergeContainerExtension(container, globalUnityResolver.GetContainer()));
                    //var container = new UnityContainer();
                    //Register(container);
                    return new UnityResolver(new UnityContainer()).GetContainer();
                });
        public static IUnityContainer Container = container.Value;

        private static ILogger _logger;
        public static ILogger Logger => _logger ?? (_logger = GetService<ILogger>());

        private static readonly Lazy<IPluginSettings> _settings = new Lazy<IPluginSettings>(() => GetService<IPluginSettings>());
        public static IPluginSettings Settings => _settings.Value;

        public static AppUser ApplicationUser { get; set; }

        public static bool Busy { get; set; }

        public static bool Initilized => InitilizedmoduleTypes.All(x => x.Value);

        public static Dictionary<string, bool> InitilizedmoduleTypes { get; set; } = new Dictionary<string, bool>();

        public static string CurrentWorkPath { get; set; }

        #endregion <props>

        #region <ctor>
        public static Plugin Create(string workPath)
        {
            return new Plugin(workPath);
        }
        private Plugin(string workPath)
        {
            CurrentWorkPath = workPath;
        }
        #endregion <ctor>

        #region <methods>

        public static void RunOnUIThread(Action action)
        {
            if (Dispatcher.CurrentDispatcher.CheckAccess())
                action();
            else
                Dispatcher.CurrentDispatcher.Invoke((Delegate)(action));
        }

        public static void RunOnUIThreadAsync(Action action)
        {
            if (Dispatcher.CurrentDispatcher.CheckAccess())
            {
                //var act = new Action(() => { Dispatcher.CurrentDispatcher.Invoke(() => action()); });
                action();
            }
            else
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
        }

        /// <summary> RunOnUIThreadAsync </summary>
        public static async Task RunOnUIThreadTaskAsync(Action action)
        {
            if (null == action) return;
            await Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }

        public static void RunOnUIThreadBackgroundAsync(Action action)
        {
            BackgroundDispatcher.BeginInvoke(new Action(() =>
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        action();
                    });
                }));
        }

        public static void DelayAction(int millisecond, Action action)
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                action.Invoke();
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
        }

        public static void StartServer()
        {
            string baseAddress = "http://localhost:8080/";
            var startup = Container.Resolve<Plugin>();
            //options.ServerFactory = "Microsoft.Owin.Host.HttpListener"
            IDisposable webApplication = WebApp.Start(baseAddress, Plugin.Configuration);

            try
            {
                Console.WriteLine("Started...");
                Console.ReadKey();
            }
            finally
            {
                webApplication.Dispose();
            }
        }

        public static void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration
            {
                DependencyResolver = new UnityResolver(Container)
            };
            //new UnityDependencyResolver(UnityHelpers.GetConfiguredContainer());

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            // Web API routes
            config.MapHttpAttributeRoutes();

            appBuilder.UseWebApi(config);

            // Add Unity filters provider
            RegisterFilterProviders(config);

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            appBuilder.UseWebApi(config);
        }

        public static IEnumerable<Type> GetTypesWithCustomAttribute<T>(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                    {
                        yield return type;
                    }
                }
            }
        }

        public static Task<bool> RegisterModuleAsync(Type moduleType, params Task[] dependTasks)
        {
            if (dependTasks != null)
            {
                //foreach (var dependTask in dependTasks)
                //{
                //    if (dependTask.Status != TaskStatus.Faulted && dependTask.Status != TaskStatus.Canceled)
                //        dependTask.Wait();
                //}

                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                try
                {
                    var dependencesTaskResult = Task.WaitAll(dependTasks, 10000, token);
                }
                catch (AggregateException ae)
                {
                    Plugin.InitilizedmoduleTypes.Add(moduleType.Name, false);
                    foreach (Exception e in ae.InnerExceptions)
                    {
                        if (e is TaskCanceledException)
                            Console.WriteLine("Unable to compute mean: {0}", ((TaskCanceledException)e).Message);
                        else
                            Console.WriteLine("Exception: " + e.GetType().Name);
                    }
                }
                finally
                {
                    source.Dispose();
                }
            }

            IRegisterModule module = Container.Resolve(moduleType) as IRegisterModule;
            if (module == null)
                throw new ArgumentException("moduleType");

            AppDomain.CurrentDomain.AssemblyResolve += AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;
            var result = module.Register(Container);
            AppDomain.CurrentDomain.AssemblyResolve -= AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;
            return result;
        }

        public static Task<bool> InitializeModuleAsync(Type moduleType, params Task[] dependTasks)
        {
            if (dependTasks != null)
            {
                foreach (var dependTask in dependTasks)
                    dependTask.Wait();
            }

            IRegisterModule module = Container.Resolve(moduleType) as IRegisterModule;
            if (module == null)
                throw new ArgumentException("moduleType");

            return module.Initialize(Container);
        }

        public Plugin InitializeInterception(Type moduleType)
        {
            //container.AddNewExtension<Interception>();
            //container.RegisterType(moduleType,
            //  new Interceptor<InterfaceInterceptor>(),
            //  new InterceptionBehavior<LoggingInterceptionBehavior>(),
            //  new InterceptionBehavior<CachingInterceptionBehavior>());
            return this;
        }

        public static T GetService<T>(string name = null) where T : class
        {
            if (name != null)
                return Container.Resolve<T>(name);
            return Container.Resolve<T>();
        }

        #endregion <methods>

        #region IRegisterModule

        public static void RegisterInstance<T>(T obj, string name = null)
        {
            Container.RegisterInstance(name, obj, new ContainerControlledLifetimeManager());
        }

        private static void RegisterFilterProviders(HttpConfiguration config)
        {
            // Add Unity filters provider
            var providers = config.Services.GetFilterProviders().ToList();
            //config.Services.Add(typeof(IFilterProvider), new WebApiUnityActionFilterProvider(UnityHelpers.GetConfiguredContainer()));
            var defaultprovider = providers.First(p => p is ActionDescriptorFilterProvider);
            config.Services.Remove(typeof(IFilterProvider), defaultprovider);
        }

        public Task<bool> Register(IUnityContainer container)
        {
            // var myAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("your_assembly_Name")).ToArray();
            container.RegisterType(typeof(Plugin));
            IPluginSettings appSettings = PluginSettings.Build(CurrentWorkPath);
            container.RegisterInstance(appSettings, new ContainerControlledLifetimeManager());

            return Task.FromResult(true);
        }

        public Task<bool> Initialize(IUnityContainer container)
        {
            return Task.FromResult(true);
        }

        #endregion IRegisterModule
    }
}