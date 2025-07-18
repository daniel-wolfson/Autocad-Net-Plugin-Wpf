using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Services.Commands;
using Intellidesk.Data.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Unity;
using Unity.Lifetime;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using RibbonButton = Autodesk.Windows.RibbonButton;
using RibbonControl = Autodesk.Windows.RibbonControl;
using RibbonPanelSource = Autodesk.AutoCAD.Customization.RibbonPanelSource;

namespace Intellidesk.AcadNet
{
    /// <summary> UIBuildService </summary>
    public class AcadNetManager : IRegisterModule
    {
        #region <public properties>

        public static Editor Ed => Doc.Editor;
        public static Database Db => HostApplicationServices.WorkingDatabase; //Doc.Database;
        public static Document Doc => acadApp.DocumentManager.MdiActiveDocument;

        private static AcadNetHttpServerHost _httpServerHostInitializer = null;
        public static AcadNetHttpServerHost HttpServerHostInitializer
        {
            get
            {
                if (_httpServerHostInitializer == null)
                    return new AcadNetHttpServerHost();
                return _httpServerHostInitializer;
            }
            set { _httpServerHostInitializer = value; }
        }

        public static AcadColors Colors { get; set; }

        public static AcadLayers Layers { get; set; }

        public static IUnityContainer Container { get; private set; }

        public static Dictionary<string, Type> PaletteTabsItems { get; set; }

        public static ICommandLine CommandLine;

        #endregion <public properties>

        #region <IRegisterModule>

        public async Task<bool> Register(IUnityContainer container)
        {
            //    Container.RegisterType<IPaletteTabCollection, PaletteTabCollection>(new TransientLifetimeManager());
            //    //new InjectionProperty("UIService")
            //    Container.RegisterType<IPanelTabView, ProjectExplorerView>("ProjectExplorer", new TransientLifetimeManager());
            //    Container.RegisterType<IPanelTabView, MapView>("MapIt", new TransientLifetimeManager());
            //    Container.RegisterType<InteractionRequestViewModel>("ProjectExplorer", new TransientLifetimeManager());

            await Initialize(container);
            return await Task.FromResult(true);
        }

        public async Task<bool> Initialize(IUnityContainer container)
        {
            PaletteTabsItems = new Dictionary<string, Type>
            {
                {"ProjectExplorer", typeof (ITabProjectExplorerView)},
                {"SerchText", typeof (IPanelTabView)}
            };

            Container = container;
            //CommandLine = container.Resolve<ICommandLine>();

            //Common.General.Linetypes.AddCableLinetype(eCableType.Cable144x12x12);
            var acadCables = new AcadCables().Cast<IPaletteElement>().ToArray();
            var acadClosures = new AcadClosures().Cast<IPaletteElement>().ToArray();
            var acadCabinets = new AcadCabinets().Cast<IPaletteElement>().ToArray();
            var acadClosureConnects = new AcadClosureConnects().Cast<IPaletteElement>().ToArray();

            Layers = new AcadLayers();
            Layers.AddLayers(acadCables);
            Layers.AddLayers(acadClosures);
            Layers.AddLayers(acadCabinets);
            Layers.AddLayers(acadClosureConnects);

            Colors = new AcadColors();
            //Colors.AddDefaultColors();
            Colors.AddColors(acadCables);
            Colors.AddColors(acadClosures);
            Colors.AddColors(acadCabinets);
            Colors.AddColors(acadClosureConnects);

            Plugin.InitilizedmoduleTypes.Add(this.GetType().Name, true);
            return await Task.FromResult(true);
        }

        #endregion

        #region <Build Ribbon control>

        /// <summary> Current ribbonTab name </summary>

        public static string CurrentRibbonTabName;


        public static ResourceManager ResourceManager;

        public static ResourceManager GetResourceManager()
        {
            return new ResourceManager("Intellidesk.Resources.Properties.Resources",
                Assembly.GetAssembly(typeof(Intellidesk.Resources.Properties.Resources)));
        }

        private static Autodesk.Windows.RibbonPanelSource XAddButonLauncher(Autodesk.Windows.RibbonPanelSource rps)
        {
            var dialogLauncherButton = new RibbonButton { Name = "TestCommand" };
            rps.DialogLauncher = dialogLauncherButton;
            return rps;
        }

        public static void RemoveRibbonTab(string ribbonTabName)
        {
            try
            {
                var ribCntrl = ComponentManager.Ribbon;
                // to do iteration by tabs
                foreach (var tab in ribCntrl.Tabs)
                {
                    if (tab.Id.Equals(ribbonTabName) & tab.Title.Equals(ribbonTabName))
                    {
                        ribCntrl.Tabs.Remove(tab);
                        // Disable the event handler
                        //Application.SystemVariableChanged -= AcadAppSystemVariableChanged;
                        break;
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
            }
        }

        #endregion

        #region <Ribbon elements>

        public static bool RibbonAdded = false;

        public static CustomizationSection GetCustomizationSection()
        {
            CustomizationSection cs = null;
            var ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                cs = new CustomizationSection((string)acadApp.GetSystemVariable("MENUNAME"));
                var curWorkspace = (string)acadApp.GetSystemVariable("WSCURRENT");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(Environment.NewLine + ex.Message);
            }
            return cs;
        }

        public static RibbonPanelSource AddRibbonPanel(CustomizationSection cs, string tabName, string panelName)
        {
            var root = cs.MenuGroup.RibbonRoot;
            var panels = root.RibbonPanelSources;

            foreach (RibbonTabSource rts in root.RibbonTabSources)
                if (rts.Name == tabName)
                {
                    //Create the ribbon panel source and add it to the ribbon panel source collection
                    var panelSrc = new RibbonPanelSource(root);
                    panelSrc.Text = panelSrc.Name = panelName;
                    panelSrc.Id = panelSrc.ElementID = panelName + panelName;
                    panels.Add(panelSrc);

                    //Create the ribbon panel source reference and add it to the ribbon panel source reference collection
                    var rps = new RibbonPanelSourceReference(rts) { PanelId = panelSrc.ElementID };
                    rts.Items.Add(rps);
                    cs.Save();
                    return panelSrc;
                }

            return null;
        }

        public static RibbonTab CreateRibbonTab(string tabName)
        {
            var rc = ComponentManager.Ribbon;
            // Look for the standard tabName
            var rt = rc.Tabs.FirstOrDefault(tab => tab.AutomationName == tabName);

            // If we didn't find it, create a custom tab
            if (rt == null)
            {
                try
                {
                    rt = new RibbonTab { Title = tabName, Id = tabName };
                    rc.Tabs.Add(rt);
                    //rt.IsActive = true;
                }
                catch (System.Exception ex)
                {
                    acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
                }
            }
            return rt;
        }

        public static RibbonControl AddRibbonTab()
        {
            var rc = new RibbonControl();

            var rt = new RibbonTab { Title = "X", Id = "X" };
            rc.Tabs.Add(rt);

            var ribSourcePanel = new Autodesk.Windows.RibbonPanelSource
            {
                Title = "X",
                DialogLauncher = new RibbonCommandItem { CommandHandler = new AdskCommandHandler() }
            };

            //Add a Panel
            var rp = new RibbonPanel { Source = ribSourcePanel };
            rt.Panels.Add(rp);

            //ribSourcePanel.Items.Add(rb1);

            return rc;
        }

        public static void AddRibbonButton()
        {
            //Create button
            //var _resourceManager = new ResourceManager("XUI.Resources", typeof(T).Assembly);
            var rb1 = new RibbonButton
            {
                Text = "Line" + "\n" + "Generator",
                CommandParameter = "Line ",
                ShowText = true,
                //LargeImage = Images.GetBitmap((Bitmap)_resourceManager.GetObject("explorer_16.bmp")),
                //Image = Images.GetBitmap((Bitmap)_resourceManager.GetObject("explorer_16.bmp")),
                Size = RibbonItemSize.Large,
                Orientation = System.Windows.Controls.Orientation.Vertical,
                ShowImage = true
            };

            rb1.ShowText = true;
            rb1.CommandHandler = new AdskCommandHandler();
        }

        public static void AddRibbonTextBox(RibbonTab rt, string title)
        {
            // Create our custom panel, add it to the ribbon tab
            var rps = new Autodesk.Windows.RibbonPanelSource { Title = title }; //"Notifying Textbox"
            var rp = new RibbonPanel { Source = rps };
            rt.Panels.Add(rp);

            // Create our custom textbox, add it to the panel
            //var tb = new WpfTextBox(150, 15, 17, 5)
            //{
            //    IsEmptyTextValid = false,
            //    AcceptTextOnLostFocus = true,
            //    InvokesCommand = true,
            //    CommandHandler = new TextboxCommandHandler()
            //};
            //rps.Items.Add(tb);

            // Set our tab to be active
            rt.IsActive = true;
        }

        #endregion

        #region <Methods>

        public static async Task<bool> InitializeHostAsync()
        {
            //return Task.Run(async () =>
            //    {
            bool isInit = await HttpServerHostInitializer.InitializeSignalRAsync();
            if (isInit)
            {
                var signalRClientHost = AcadNetHttpServerHost.SignalRClientHost;
                if (!signalRClientHost.IsConnected)
                    await signalRClientHost.LoadHostAsync();
            }
            return isInit;
            //    });
        }

        public static List<Task> InitializeHostAsyncTemp()
        {
            HttpServerHostInitializer = new AcadNetHttpServerHost();
            var tasks = new List<Task>
            {
                //Task.Run(async () => await HttpServerHostInitializer.InitializeWebApiAsync()),
                //Task.Run(async () => await HttpServerHostInitializer.InitializeJobSchedulerAsync()),
            };
            return tasks;

            //awai;;t Task.WhenAll(tasks).ConfigureAwait(false);
            //Task.Run(async () => await HttpServerHostInitializer.InitializeWebApiAsync());
            //Task.Run(async () => await HttpServerHostInitializer.InitializeJobSchedulerAsync());
            //Task.Run(async () =>
            //{
            //    bool isInit = await HttpServerHostInitializer.InitializeSignalRAsync();
            //    if (isInit)
            //    {
            //        var signalRClientHost = AcadNetHttpServerHost.GetSignalRClientHost();
            //        if (!signalRClientHost.IsConnected)
            //            signalRClientHost.LoadHostAsync();
            //    }
            //}
            //acadApp.DocumentManager.MdiActiveDocument.SendStringToExecute("RELAYCLIENTHOST ", true, false, false);
            //try { Task.Run(() => NetAclChecker.AddAddress(appSettings.IntelliDeskHost)); }
            //catch (Exception) { }
            //var aggregator = Plugin.GetService<IEventAggregator>();
            //aggregator.GetEvent<ObjectIdMessageEvent>().Subscribe(ExecuteSaveGeoDataAs);
            //);
        }

        public static void Clean()
        {
            foreach (var registration in Container.Registrations
                .Where(p => p.RegisteredType == typeof(ContainerControlledLifetimeManager))) //.LifetimeManagerType
            {
                registration.LifetimeManager.RemoveValue();
            }

            Container = null;
        }

        /// <summary> Get image from resources </summary>
        private static BitmapImage LoadImage(string imageName)
        {
            return new BitmapImage(new Uri("pack://application:,,,/X;component/Resources/" + imageName + ".png"));
        }

        /// <summary> WriteMessage </summary>
        public static void WriteMessage(string s)
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage(s);
        }

        public static void Alert(string alert)
        {
            acadApp.ShowAlertDialog(alert);
        }

        //public static WaitWindow CreateWaitWindow(string cmd = "", object Args = null)
        //{
        //    WaitWindow waitWindow = null;
        //    _manualResetEvent = new ManualResetEvent(false);

        //    // Create a new thread for the splash screen to run on
        //    _waitWindowThread = new Thread(() =>
        //    {
        //        waitWindow = _unityContainer.Resolve<WaitWindow>();
        //        //Application.ShowModelessWindow(Application.MainWindow.Handle, waitWindow, false);
        //        waitWindow.Show();

        //        // Now that the window is created, allow the rest of the startup to run
        //        _manualResetEvent.Set();
        //        System.Windows.Threading.Dispatcher.Run();

        //        if (cmd != "")
        //        {
        //            CommandLine.SendToExecute(cmd); 
        //        }
        //    });

        //    _waitWindowThread.SetApartmentState(ApartmentState.STA);
        //    _waitWindowThread.IsBackground = true;
        //    _waitWindowThread.Name = "Plugin welcome screen";
        //    _waitWindowThread.Start();

        //    _manualResetEvent.WaitOne();

        //    return waitWindow;
        //}

        #endregion
    }
}