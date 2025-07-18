using AspNet.Identity.PostgreSQL;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using FileExplorer.Model;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Components;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.AcadNet.Services;
using Intellidesk.AcadNet.Services.Jobs;
using Intellidesk.AcadNet.ViewModels;
using Intellidesk.AcadNet.Views;
using Intellidesk.AcadNet.WebBrowser;
using Intellidesk.Data.Auth;
using Intellidesk.Data.General;
using Intellidesk.Data.Models.DataContext;
using Intellidesk.Data.Repositories;
using Intellidesk.Data.Repositories.EF6.DataContext;
using Intellidesk.Data.Repositories.EF6.Factories;
using Intellidesk.Data.Repositories.EF6.UnitOfWork;
using Intellidesk.Data.Services;
using Prism.Events;
using Prism.Ioc;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using ConfigurationBuilder = ID.Infrastructure.ConfigurationBuilder;
using Exception = System.Exception;
using IConfigurationBuilder = ID.Infrastructure.IConfigurationBuilder;
using IPanelTabView = Intellidesk.AcadNet.Common.Interfaces.IPanelTabView;
using SystemVariableChangedEventArgs = Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventArgs;

[assembly: ExtensionApplication(typeof(AcadNetPlugin))]
namespace Intellidesk.AcadNet
{
    public class AcadNetPlugin : IExtensionApplication, IRegisterModule
    {
        #region <IExtensionApplication>

        void IExtensionApplication.Initialize()
        {
            // 1. Export a function from unmanaged code that takes a function
            //    pointer and stores the passed in value in a global variable.
            // 2. Call this exported function in this function passing delegate.
            // 3. When unmanaged code needs the services of this managed module
            //    you simply call acrxLoadApp() and by the time acrxLoadApp 
            //    returns  global function pointer is initialized to point to
            //    the C# delegate.
            // For more info see: 
            // http://msdn2.microsoft.com/en-US/library/5zwkzwf4(VS.80).aspx
            // http://msdn2.microsoft.com/en-us/library/44ey4b32(VS.80).aspx
            // http://msdn2.microsoft.com/en-US/library/7esfatk4.aspx
            // as well as some of the existing AutoCAD managed apps.
            // Initialize your plug-in application here

            // Connect the event handler change system variables
            //Application.QuitWillStart += Application_QuitWillStart;
            //ComponentManager.UIElementActivated += ComponentManagerUIElementActivated;

            //App.SystemVariableChanged += AcadAppSystemVariableChanged;
            ////App.Idle += Application_Idle;
            //App.DocumentManager.DocumentActivated += Commands.OnDocumentActivated;
            //App.DocumentManager.DocumentToBeDestroyed += Commands.OnDocumentToBeDestroyed;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += (sender, args) =>
            {
                Exception e = (Exception)args.ExceptionObject;
                acadApp.DocumentManager.CurrentDocument?.Editor.WriteMessage("UnhandledException : " + e.Message);
                Plugin.Logger.Error("UnhandledException : " + e.Message);
            };

            ComponentManager.ItemInitialized += ComponentManagerItemInitialized;

            //DbConfiguration.Loaded += (_, a) =>
            //{
            //    var a1 = 1;
            //    //a.ReplaceService<DbProviderServices>((s, к) => new MyProviderServices(s));
            //    //a.ReplaceService<IDbConnectionFactory>((s, к) => new MyConnectionFactory(с));
            //};
            //EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent,
            //    new RoutedEventHandler((object sender, RoutedEventArgs e) => { (sender as TextBox).SelectAll(); }));

            #region "EventLog - working only user is administrator on current machine"

            //if (!EventLog.SourceExists(ProjectManager.Name))
            //{
            //    //An event log source should not be created and immediately used.
            //    //There is a latency time to enable the source, it should be created
            //    //prior to executing the application that uses the source.
            //    //Execute this sample a second time to use the new source.
            //    EventLog.CreateEventSource(ProjectManager.Name, "Intel");
            //    // The source is created.  Exit the application to allow it to be registered.
            //}

            //// Create an EventLog instance and assign its source.
            //var myLog = new EventLog { Source = ProjectManager.Name };
            //// Write an informational entry to the event log.    
            //myLog.WriteEntry(ProjectManager.Name + " starting...");
            //Log.EventViewerEnabled = true;

            #endregion

            //StateService.Start("http://localhost/StateService/StateService.svc");
        }

        void IExtensionApplication.Terminate()
        {
            //var aggregator = Plugin.GetService<IEventAggregator>();
            //aggregator.GetEvent<NotifyMessageEvent>().Unsubscribe(ComponentManager.Ribbon.DisplayNotifyMessage);
            //DatabaseBackUp(ConfigurationManager<AppDbContext>.GetConfiguration());
            //lifetimeManagerAggregator.RemoveValue();
            //lifetimeManagerLog.RemoveValue();
            //p.RegisteredType == typeof(object) &&

            try
            {
                Plugin.Busy = false;
                Plugin.Settings.Save();

                if (WebBrowserInitializer.WebBrowser != null)
                {
                    WebBrowserInitializer.WebBrowser.Dispose();
                    WebBrowserInitializer.WebBrowser = null;
                }

                AcadNetManager.HttpServerHostInitializer.Terminate();

                Dispatcher.ExitAllFrames();

                Process[] acadProcesses = Process.GetProcessesByName("acad");
                foreach (Process acadProcess in acadProcesses)
                {
                    acadProcess.CloseMainWindow();
                }
                //Process process = Process.GetProcessById(Plugin.signalRProcessId);
                //if (!process.HasExited)
                //    process.Kill();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion <IExtensionApplication>

        #region <IRegisterModule>

        public async Task<bool> Register(IUnityContainer container)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "IntelliDesk");
            builder.SetBasePath(basePath);
            builder.AddJsonFile("appsettings");
            IConfigurationBuilder config = builder.Build();

            container.RegisterInstance(config, new ContainerControlledLifetimeManager());
            container.RegisterInstance(new SerilogLoggerFactory(null, false));

            //appsettings
            var appSettings = config.GetSection<PluginSettings>();
            appSettings.Prompt = $"\n{CommandNames.UserGroup.ToCamelCase()}:{char.ConvertFromUtf32(160)}";
            container.RegisterInstance<IPluginSettings>(appSettings, new ContainerControlledLifetimeManager());

            var appConfig = config.GetSection<AppConfig>();
            container.RegisterInstance<IAppConfig>(appConfig, new ContainerControlledLifetimeManager());
            var authOptions = config.GetSection<AuthOptions>();

            container.RegisterInstance<IAuthOptions>(authOptions, new ContainerControlledLifetimeManager());

            //logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(PluginSettings.WorkPath + $"//Logs//{DateTime.Now.ToString("yyyy-MM-dd")}.log")
                .CreateLogger();
            ILogger logger = Log.Logger;
            container.RegisterInstance(logger, new ContainerControlledLifetimeManager());

            // Configure PostSharp Logging to use Serilog
            //LoggingServices.DefaultBackend = new SerilogLoggingBackend(logger);

            // set folder
            var folder = new LocalVirtualFolder().SetFoldersAsync(appSettings.IncludeFolders.ToArray());

            // EventAggregator
            container.RegisterType<ITaskArguments, TaskArguments>(new TransientLifetimeManager());
            container.RegisterInstance((IEventAggregator)new EventAggregator(), new ContainerControlledLifetimeManager());
            container.RegisterType<ILayerService, LayerService>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICommandLine, CommandLine>(new TransientLifetimeManager());
            //new InjectionProperty("Logger"));

            //Data
            container.RegisterType<IDataContextAsync, IntelliDesktopContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<IRepositoryFactories, RepositoryFactories>(new ContainerControlledLifetimeManager());
            container.RegisterType<IRepositoryProvider, RepositoryProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IUnitOfWorkAsync, UnitOfWork>(new ContainerControlledLifetimeManager());

            //ViewModels
            container.RegisterType<IProjectExplorerPanelContext, ProjectExplorerPanelContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISearchTextPanelContext, SearchTextPanelContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<ILayerQueriesPanelContext, LayerQueriesPanelContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<IBayQueriesPanelContext, BayQueriesPanelContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICablePanelContext, CablePanelContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<IClosurePanelContext, ClosurePanelContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<IClosureConnectPanelContext, ClosureConnectPanelContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICabinetPanelContext, CabinetPanelContext>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMapViewModel, MapViewModel>(new ContainerControlledLifetimeManager());

            //Services
            container.RegisterType<IFilterService, FilterService>(new TransientLifetimeManager());
            container.RegisterType<IDataConfigService, DataConfigService>(new TransientLifetimeManager());
            container.RegisterType<ILayoutService, LayoutService>(new TransientLifetimeManager());

            container.RegisterInstance(Services.DrawService.GetInstance(), new ContainerControlledLifetimeManager());
            container.RegisterType<IUserRepository, UserRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<AuthRepository>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAdminService<IdentityUserDetails>, UserService>(new ContainerControlledLifetimeManager());

            ToolsManager.Initialize(container);

            container.RegisterType<CustomRibbonTab>(PaletteNames.PartnerTab.ToString(), new ContainerControlledLifetimeManager());

            //views
            container.RegisterType<IPanelTabView, CablePanelView>(PaletteNames.Cable.ToString(), new TransientLifetimeManager(),
                new InjectionConstructor(new Func<ICablePanelContext>(() => container.Resolve<ICablePanelContext>())));
            container.RegisterType<IPanelTabView, ClosurePanelView>(PaletteNames.Closure.ToString(), new TransientLifetimeManager(),
                new InjectionConstructor(new Func<IClosurePanelContext>(() => container.Resolve<IClosurePanelContext>())));
            container.RegisterType<IPanelTabView, CabinetPanelView>(PaletteNames.Cabinet.ToString(), new TransientLifetimeManager(),
                new InjectionConstructor(new Func<ICabinetPanelContext>(() => container.Resolve<ICabinetPanelContext>())));
            container.RegisterType<IPanelTabView, ClosureConnectPanelView>(PaletteNames.ClosureConnect.ToString(), new TransientLifetimeManager(),
                new InjectionConstructor(new Func<IClosureConnectPanelContext>(() => container.Resolve<IClosureConnectPanelContext>())));

            container.RegisterType<IPanelTabView, SearchTextView>(PaletteNames.Search.ToString(), new TransientLifetimeManager());
            container.RegisterType<IPanelTabView, ProjectExplorerView>(PaletteNames.ProjectExplorer.ToString(), new TransientLifetimeManager());

            container.RegisterType<IPanelTabView, QueryLayersPanelView>(PaletteNames.LayerQueries.ToString(), new TransientLifetimeManager(),
                new InjectionConstructor(new Func<ILayerQueriesPanelContext>(() => container.Resolve<ILayerQueriesPanelContext>())));
            container.RegisterType<IPanelTabView, QueryBaysPanelView>(PaletteNames.BayQueries.ToString(), new TransientLifetimeManager(),
                new InjectionConstructor(new Func<IBayQueriesPanelContext>(() => container.Resolve<IBayQueriesPanelContext>())));
            container.RegisterType<IPanelTabView, MapView>(PaletteNames.MapView.ToString(), new ContainerControlledLifetimeManager());

            //container.RegisterType<CustomRibbonTab>(PanelNames.PartnerTab.ToString(), new ContainerControlledLifetimeManager(),
            //    new InjectionConstructor(container.Resolve<IEventAggregator>(),
            //            container.Resolve<IProjectExplorerPanelContext>(),
            //            container.Resolve<ICommandLine>(), appSettings));

            //var provider = new DpapiDataProtectionProvider("IntelliDesk");
            //var userTokenProvider = new DataProtectorTokenProvider<AppUser, string>(provider.Create("PasswordReset"));
            //container.RegisterInstance(userTokenProvider, new ContainerControlledLifetimeManager());

            await Initialize(container);
            return await Task.FromResult(true);
        }

        public async Task<bool> Initialize(IUnityContainer container)
        {
            Plugin.InitilizedmoduleTypes.Add(this.GetType().Name, true);
            return await Task.FromResult(true);
        }

        #endregion <IRegisterModule>

        #region <Callbacks>

        /// <summary> Building ribbon Event handler, occurs after plugin initialized </summary>
        void ComponentManagerItemInitialized(object s, RibbonItemEventArgs e)
        {
            ComponentManager.ItemInitialized -= ComponentManagerItemInitialized;

            acadApp.QuitAborted += (sender, args) =>
            {
                Plugin.Busy = false;
            };
            acadApp.BeginQuit += (sender, args) =>
            {
                ToolsManager.CleanEventsAndClose();
                acadApp.SystemVariableChanged -= OnSystemVariableChanged;
                acadApp.Idle -= OnApplicationIdle;
                AcadNetManager.HttpServerHostInitializer.Terminate();
                acadApp.SystemVariableChanged -= OnSystemVariableChanged;
                acadApp.Idle -= OnApplicationIdle;
                Edit.ObjectErased -= Edit.OnAcadEntityErased;
                Edit.ObjectModified -= Edit.OnAcadEntityModified;

                EmailScheduler.Clear();

                var signalRClientHost = AcadNetHttpServerHost.SignalRClientHost;
                if (signalRClientHost.IsConnected)
                {
                    signalRClientHost.Terminate();
                    signalRClientHost.Dispose();
                }
            };
            acadApp.BeginCloseAll += (sender, args) =>
            {
                acadApp.DocumentManager.DocumentActivated -= OnDocumentActivated;
                acadApp.DocumentManager.ExecuteInApplicationContext(data =>
                {
                    foreach (Document doc in acadApp.DocumentManager)
                    {
                        doc.UserData.Clear();
                    }
                }, null);
            };
            acadApp.QuitWillStart += (sender, args) =>
            {
                Plugin.Busy = true;
            };

            Edit.ObjectErased += Edit.OnAcadEntityErased;
            Edit.ObjectModified += Edit.OnAcadEntityModified;

            acadApp.SystemVariableChanged += OnSystemVariableChanged;
            acadApp.DocumentManager.DocumentToBeDestroyed += OnDocumentToBeDeactivatedOrDestroyed;
            acadApp.DocumentManager.DocumentToBeDeactivated += OnDocumentToBeDeactivatedOrDestroyed;
            acadApp.DocumentManager.DocumentActivated += OnDocumentActivated;

            AppPane appPane = new AppPane();
            var taskAcadNetRegister = Plugin.RegisterModuleAsync(typeof(AcadNetPlugin));
            var taskDatatRegister = Plugin.RegisterModuleAsync(typeof(AcadNetDataManager), taskAcadNetRegister);

            var commandTasks = new List<Task>
            {
                AcadNetManager.InitializeHostAsync(),
                Plugin.RegisterModuleAsync(typeof(AcadNetManager), taskDatatRegister),
                taskDatatRegister,
                taskAcadNetRegister
            };

            acadApp.DocumentManager.MdiActiveDocument
                .SendCommandToExecute(null, CommandNames.PluginLoad, commandTasks);
        }

        /// <summary>
        /// handle to event of changed system variable. 
        /// We will monitor the system variable WSCURRENT (current workspace),
        /// his tab is not "lost" when editing a workspace
        /// </summary>
        void OnSystemVariableChanged(object sender, SystemVariableChangedEventArgs e)
        {
            if (e.Name.Equals("WSCURRENT"))
            {
                RibbonControl rc = ComponentManager.Ribbon;
                if (rc != null && rc.IsVisible && !rc.ContainsTab(Plugin.Settings.Name))
                    acadApp.DocumentManager.MdiActiveDocument
                        .SendStringToExecute(CommandNames.PluginLoad + " ", true, false, false);
            }
            //else if (e.Name.Equals("RIBBONSTATE")) { }
        }

        private void OnDocumentActivated(object sender, DocumentCollectionEventArgs args)
        {
            try
            {
                var doc = ((DocumentCollection)sender)?.MdiActiveDocument;
                if (doc != null)
                {
                    acadApp.DocumentManager.MdiActiveDocument = doc;

                    IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();

                    if (!doc.Database.IsRegAppTableRecord(pluginSettings.Name))
                        acadApp.DocumentManager.GetDocument(doc.Database)
                            .AddRegAppTableRecord(pluginSettings.Name);

                    //DwgVersion version = doc.Database.OriginalFileVersion; //https://knowledge.autodesk.com/support/autocad/learn-explore/caas/sfdcarticles/sfdcarticles/drawing-version-codes-for-autocad.html
                    //if (version == DwgVersion.AC1032) //2018
                    //    using (doc.LockDocument())
                    //    doc.Database.SaveAs(doc.Database.OriginalFileName, DwgVersion.Current); //version = Db.OriginalFileVersion;

                    var ps = Plugin.GetService<IPluginSettings>();
                    doc.AddRegAppTableRecord(ps.Name);

                    acadApp.DocumentManager.MdiActiveDocument
                        .SendCommandToExecute(ToolsManager.PaletteTabs.Current, CommandNames.XLayersLoadData);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.ErrorEx(ex);
            }
        }

        private void OnDocumentToBeDeactivatedOrDestroyed(object sender, DocumentCollectionEventArgs args)
        {
            try
            {
                var doc = ((DocumentCollection)sender).MdiActiveDocument ?? acadApp.DocumentManager.CurrentDocument;
                if (doc != null)
                    doc.UserData.Clear();
            }
            catch (Exception ex)
            {
                Plugin.Logger.ErrorEx(ex);
            }
        }

        public static void OnApplicationIdle(object sender, EventArgs e)
        {
            try
            {
                var doc = acadApp.DocumentManager.MdiActiveDocument;

                // Remove the event handler as it is no longer needed
                if (sender != null || doc == null) return;

                if (doc.UserData.Keys.Cast<string>().Any(x => x.StartsWith("IDLE")))
                {
                    if (doc.UserData.ContainsKey(CommandNames.XFileTempSave))
                    {
                        dynamic dynamicCommandArgs = acadApp.DocumentManager.MdiActiveDocument.UserData[CommandNames.XFileTempSave];
                    }
                    else if (doc.UserData.ContainsKey(CommandNames.XIdleOnHubMessage))
                    {
                        var pluginSettings = Plugin.GetService<IPluginSettings>();
                        dynamic message = acadApp.DocumentManager.MdiActiveDocument.UserData[CommandNames.XIdleOnHubMessage];
                        doc.Editor.WriteMessage(pluginSettings.Prompt + message.ToString());
                        acadApp.DocumentManager.MdiActiveDocument.UserData.Remove(CommandNames.XIdleOnHubMessage);
                        Autodesk.AutoCAD.Internal.Utils.PostCommandPrompt();
                    }
                    else if (doc.UserData.ContainsKey(CommandNames.XIdleOnHubDisconnected))
                    {
                        doc.UserData.Remove(CommandNames.XIdleOnHubDisconnected);
                        doc.SendStringToExecute(CommandNames.XIdleOnHubDisconnected + " ", true, false, false);
                    }

                    if (!doc.UserData.Keys.Cast<string>().Any(x => x.StartsWith("IDLE")))
                        acadApp.Idle -= OnApplicationIdle;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
            }
            finally
            {
                //acadApp.DocumentManager.MdiActiveDocument.UserData.Remove(CommandNames.XFileTempSave);
            }
        }

        #endregion <Callbacks>

        #region <utils>

        public static void LoadServiceClientAssembly(string dllFileName)
        {
            var pathName = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (pathName == null) return;

            string path = pathName.Replace("Win64", "") + $"BinClient\\{dllFileName}";
            LoadAssemblyDll(path);
        }

        public static void LoadAssemblyDll(string dllFileName)
        {
            string dll = dllFileName;
            if (!dllFileName.Contains("\\"))
            {
                string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                dll = path + "\\" + dllFileName + ".dll";
            }

            if (!ExtensionLoader.IsLoaded(dll))
            {
                if (System.IO.File.Exists(dll))
                {
                    Assembly assembly = ExtensionLoader.Load(dll);
                    //Ed.WriteMessage($"{pluginSettings.Prompt}assembly {assembly.GetName().Name} loaded");
                }
                else
                    throw new System.IO.FileNotFoundException("Cannot find file: " + dllFileName + "!");
            }
        }

        #endregion <utils>
    }
}

//using (new SysVarOverride("SECURELOAD", 0))
//{
//    AcadNetManager.LoadServiceClientAssembly("ID.AcadNet.SignalRClient.dll");
//    //acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("RELAYCLIENTHOST ", true, false, false);
//    //AcadNetManager.LoadAssemblyDll("D:\\IntelliDesk\\IntelliDesk.bundle\\Contents\\BinHost\\ID.SignalRSelfHost.Lib.dll");
//}
//var myAttribute = GetType().GetMethod("RibbonLoad").GetCustomAttributes(true).OfType<CustomCommandMethodAttribute>().FirstOrDefault();
