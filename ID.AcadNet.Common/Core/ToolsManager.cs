using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Threading;
using Unity;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Core
{
    public class ToolsManager
    {
        private static IUnityContainer Container { get; set; }
        public static IPluginSettings PluginSettings => Container.Resolve<IPluginSettings>();
        private static bool IsPaletteTabsClosable = false;

        public static readonly RibbonControl Rc = ComponentManager.Ribbon;
        private static Dispatcher UiDispatcher => Dispatcher.CurrentDispatcher;

        private static readonly Dictionary<PaletteNames, Func<PaletteNames, ICommandArgs, IPanelTabView>> TabActions =
                    new Dictionary<PaletteNames, Func<PaletteNames, ICommandArgs, IPanelTabView>>()
                    {
                        { PaletteNames.ProjectExplorer, CreatePalette },
                        { PaletteNames.Search, CreatePalette },
                        { PaletteNames.Cable, CreatePalette },
                        { PaletteNames.Closure, CreatePalette },
                        { PaletteNames.Cabinet, CreatePalette },
                        { PaletteNames.MapView, CreatePalette }
                    };

        private ToolsManager()
        {
        }

        #region "PaletteSets"

        private static readonly PaletteSetStateEventHandler _onPaletteSetChanged = null;

        private static IPaletteTabCollection _paletteTabs;

        public static IPaletteTabCollection PaletteTabs
        {
            get
            {
                if (_paletteTabs == null)
                {
                    _paletteTabs = new PaletteTabCollection();
                    if (_paletteTabs != null)
                        _paletteTabs.PaletteSetChanged += (sender, args) =>
                        {
                            ICommandLine commandLine = Container.Resolve<ICommandLine>();
                            if (args.NewState == StateEventIndex.Hide && IsPaletteTabsClosable)
                                commandLine.SendToExecute(CommandNames.XCheckPalettesetClose + " ");

                            var viewModel = Plugin.GetService<IProjectExplorerPanelContext>();
                            viewModel.IsTabButtonActive = args == EventArgs.Empty || args.NewState == StateEventIndex.Hide;

                            //if (ComponentManager.Ribbon != null && ComponentManager.Ribbon.IsVisible)
                            //{
                            //    if (ComponentManager.Ribbon.CurrentAutoHideMode == RibbonAutoHideMode.None)
                            //        ComponentManager.Ribbon.Focus();
                            //}
                        };
                }
                return _paletteTabs;
            }
            private set
            {
                _paletteTabs = value;
            }
        }

        public static void PaletteTabClose(object sender, EventArgs e)
        {
            if (e != EventArgs.Empty) return;
            _paletteTabs.PaletteSetChanged -= _onPaletteSetChanged;
            _paletteTabs.RootPaletteSet = null;
            _paletteTabs = null;
        }

        #endregion

        [CommandAspect]
        public static void LoadPallete(PaletteNames paletteTabName, ICommandArgs args,
            string loadCommandName = null, Action<IPanelTabView,
            ICommandArgs> afterCallBack = null)
        {
            PaletteTabs.RootPaletteSet.Visible = true;

            var paletteTab = PaletteTabs[paletteTabName];
            if (paletteTab == null)
                paletteTab = CreatePalette(paletteTabName, args);
            else
            {
                if (!paletteTab.IsActive)
                    PaletteTabs.Activate(paletteTabName, args);
                else if (args != null)
                {
                    paletteTab.ActivateArgument = args;
                    paletteTab.OnActivate(args);
                }
            }

            var size = PaletteTabs.RootPaletteSet.GetSize();
            PaletteTabs.IsLoaded = false;
            PaletteTabs.RootPaletteSet.SetSize(new Size(PluginSettings.ToolPanelWidth == 0
                ? PluginSettings.ToolPanelLastWidth : PluginSettings.ToolPanelWidth, size.Height));
            PaletteTabs.IsLoaded = true;
            PaletteTabs.RootPaletteSet.Dock = DockSides.Left;
            PaletteTabs.RootPaletteSet.Visible = true;
            PaletteTabs.RootPaletteSet.KeepFocus = true;

            afterCallBack?.Invoke(PaletteTabs[paletteTabName], args);
        }

        private static IPanelTabView CreatePalette(PaletteNames paletteTabName, ICommandArgs args)
        {
            IPanelTabView paletteTab;
            try
            {
                paletteTab = Plugin.GetService<IPanelTabView>(paletteTabName.ToString());
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex?.InnerException.Message ?? ex.Message);
                return null;
            }

            paletteTab.Name = paletteTabName.ToString();
            paletteTab.ActivateArgument = args;

            if (!PaletteTabs.ContainsTab(paletteTabName))
                PaletteTabs.AddTab(paletteTab);

            if (!paletteTab.IsActive)
                PaletteTabs.Activate(paletteTabName);

            return paletteTab;
        }

        public static ResourceManager GetResourceManager()
        {
            return new ResourceManager("Intellidesk.Resources.Properties.Resources",
                Assembly.GetAssembly(typeof(Intellidesk.Resources.Properties.Resources)));
        }

        public static void Initialize(IUnityContainer unityContainer)
        {
            Container = unityContainer;
        }

        public static void CleanEventsAndClose()
        {
            foreach (IPanelTabView tab in PaletteTabs)
            {
                PaletteTabs.CloseTab(tab, false);
            }
            //PaletteTabs.RemoveAllEvents();
            PaletteTabs = null;
            Container = null;
        }

        public static void Common_Idle(object sender, EventArgs e)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            if (sender == null && Plugin.Initilized)
            {
                if (doc.UserData.Keys.Cast<string>().Any(x => x.StartsWith("COMMON_IDLE")))
                {
                    if (doc.UserData.ContainsKey("COMMON_IDLE_CABLE"))
                    {
                        doc.Editor.WriteMessage(PluginSettings.Prompt + "COMMON_IDLE_CABLE");
                        LoadPallete(PaletteNames.Cable, doc.UserData["COMMON_IDLE_CABLE"] as ICommandArgs, null);
                        doc.UserData.Remove("COMMON_IDLE_CABLE");
                    }

                    if (!doc.UserData.Keys.Cast<string>().Any(x => x.StartsWith("COMMON_IDLE")))
                        acadApp.Idle -= Common_Idle;
                }

            }
        }

        private static void LoadExplorerCallBack(object sender, ICommandArgs args)
        {
            //Mouse.OverrideCursor = null;
            //Mouse.OverrideCursor = Cursors.Wait;
            var folder = args as ICommandArgs;
            //if (folder == null) return;
            //folder.IsExpanded = true;
            //folder.IsSelected = true;
            //var palleteTab = Sender as ITabProjectExplorerView;
            //if (palleteTab == null) return;
            //Commands.DelayAction(500, () => { palleteTab.ExpandFolder(folder, true); });
            //Mouse.OverrideCursor = null;
        }
    }
}