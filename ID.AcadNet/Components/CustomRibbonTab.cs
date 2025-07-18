#define PARTNER //INTEL

using Autodesk.Windows;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Utils;
using Intellidesk.AcadNet.Core;
using Intellidesk.AcadNet.ViewModels;
using Intellidesk.Data.Models.Cad;
using Intellidesk.Resources;
using Prism.Events;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Components
{
    public class CustomRibbonTab : RibbonTab
    {
        private readonly ProjectExplorerPanelContext _context;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPluginSettings _pluginSettings;
        private readonly ICommandLine _commandLine;
        private RibbonButton _notificationsButton;
        private Dictionary<string, BitmapImage> _resourceImages = new Dictionary<string, BitmapImage>();

        public CustomRibbonTab() { }

        public CustomRibbonTab(IEventAggregator ea,
            IProjectExplorerPanelContext pep,
            IPluginSettings pluginSettings,
            ICommandLine commandLine)
        {
            _context = pep as ProjectExplorerPanelContext;
            _pluginSettings = pluginSettings;
            Name = pluginSettings.Name;
            Title = Id = pluginSettings.Name;
            _eventAggregator = ea;
            _eventAggregator.GetEvent<NotifyMessageEvent>().Subscribe(OnNotifyMessage);
            _commandLine = commandLine;
        }

        /// <summary> Creare RibbonTab </summary>
        public void AddControls()
        {
            var rm = Intellidesk.Resources.Properties.Resources.ResourceManager;

            #region <Project's Explorer>

            // create panel source and create panel
            var panelTools = new RibbonPanelSource { Title = "Project's Explorer", Name = "Explorer", Id = "Explorer" };
            panelTools.Items.Add(new RibbonButton
            {
                Text = "Explorer",
                Name = "Explorer",
                Id = "RbExplorerId",
                CommandHandler = _context != null ? _context.RbnOpenExplorer : null,
                ShowText = true,
                LargeImage = rm.GetBitmapImage("explorer", 2, 1),
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                ShowImage = true,
                ToolTip = new RibbonToolTip
                {
                    Title = "Project Explorer",
                    IsHelpEnabled = false,
                    Content = "Browse, load and manage panels tools"
                }
            });

            // Create a Command Item that the Dialog Launcher can use, for this test it is just a place holder.
            var bitmapImage = rm.GetBitmapImage("prop_arrow");
            var dialogLauncherButton = new RibbonButton
            {
                Name = "Options",
                Image = bitmapImage,
                LargeImage = bitmapImage
            };
            panelTools.DialogLauncher = dialogLauncherButton;

            #region "InitProjects"

            //layoutComboBox.DropDownOpened += OnlayoutComboBoxDropDownOpened;
            //layoutComboBox.DropDownClosed += OnlayoutComboBoxDropDownClosed;
            //layoutComboBox.CurrentChanged += mainPanel.OnlayoutComboBoxCurrentChanged;

            //var bind = new Binding("RibbonLayoutItems") { Source = mainPanel };
            //layoutComboBox.ItemsBinding = bind;
            //layoutComboBox.Current = mainPanel.RibbonLayoutItems.FirstOrDefault(X => X.Id == "none");
            //bind = new Binding("CurrentRibbonLayout") { Source = mainPanel };
            //layoutComboBox.CurrentBinding = bind;

            #endregion

            var rowPanelActivities = new RibbonRowPanel() { Name = "PanelActivities", Id = "PanelActivities" };
            rowPanelActivities.Items.Add(new RibbonLabel() { Text = "Projects:", Width = 85 });
            var layoutComboBox = new RibbonCombo
            {
                Name = "ActiveProject",
                Id = "RbnLayoutsId",
                Width = 150,
                ToolTip = new RibbonToolTip
                {
                    Title = "Active Project",
                    IsHelpEnabled = false,
                    Content = "Select and switch to one of the drawings currently open in AutoCAD"
                }
            };

            rowPanelActivities.Items.Add(layoutComboBox);

            #region "InitSettings"

            var settingsGalleryBox = new RibbonCombo
            {
                Name = "Settings Gallery",
                Id = "SettingsGalleryId",
                Width = 150,
                ToolTip = new RibbonToolTip
                {
                    Title = "Active Settings",
                    IsHelpEnabled = false,
                    Content = "The configuration set currently being used for layout catalog options"
                }
            };

            RibbonButton rb; RibbonToolTip tt;

            var configs = _context != null && _context.ConfigItems != null
                ? _context.ConfigItems.ToList() : new List<Config>();

            if (configs.Count != 0)
            {
                foreach (var config in configs)
                {
                    rb = new RibbonButton
                    {
                        Name = "ConfigItem_" + config.ConfigSetName,
                        CommandParameter = config,
                        CommandHandler = _context != null ? _context.RbnOpenConfigCommand : null,
                        Orientation = Orientation.Horizontal,
                        Size = RibbonItemSize.Standard,
                        Text = config.ConfigSetName,
                        ShowImage = false,
                        ShowText = true,
                        Tag = "",
                        Height = 40
                    };

                    var frameLayerTypes = new List<string>();
                    config.LayoutOptions.Where(x => x.ParameterName == "FRAME_TYPE_ID").ToList()
                        .ForEach(conf =>
                        {
                            var layerTypes = config.LayoutOptions
                                .Where(x => x.ParameterName == "LAYER_TYPE" && x.Key.ToString() == conf.Key.ToString())
                                .Select(x => x.Value).ToArray();

                            if (layerTypes.Any())
                                frameLayerTypes.Add("  FRAME_TYPE_ID=" + conf.Key.ToString().Trim() + "," + string.Join(",", layerTypes));
                        });

                    var attrs = string.Join(", ", config.LayoutOptions
                        .Where(x => x.ParameterName == "TOOL_NAME_ATTRIBUTE").Select(x1 => x1.Value).ToArray());

                    var toolTipString = string.Format("LAYER_TYPES :\n{0};\n\nTOOL_NAME_ATTRIBUTES :\n  {1}", string.Join(";\n ", frameLayerTypes), attrs);
                    tt = new RibbonToolTip
                    {
                        Title = config.ConfigSetName,
                        IsHelpEnabled = false,
                        Command = "Command: CONFIG ",
                        Content = toolTipString
                    };
                    rb.ToolTip = tt;

                    settingsGalleryBox.Items.Add(rb);

                    if (config.ConfigSetName == _context.CurrentConfig.ConfigSetName)
                        settingsGalleryBox.Current = rb;
                }
            }
            else
            {
                tt = new RibbonToolTip { IsHelpEnabled = false };
                rb = new RibbonButton
                {
                    Name = "ConfigItem_",
                    CommandParameter = tt.Command = "",
                    CommandHandler = null,
                    Orientation = Orientation.Horizontal,
                    Size = RibbonItemSize.Standard,
                    Height = 28,
                    Text = tt.Title = "None",
                    ShowImage = false,
                    ShowText = true,
                    Tag = ""
                };
                tt.Content = "Configurations not found";
                rb.ToolTip = tt;
                settingsGalleryBox.Items.Add(rb);
            }

            #endregion

            rowPanelActivities.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelActivities.Items.Add(new RibbonLabel() { Text = "Settings:", Width = 85 });
            rowPanelActivities.Items.Add(settingsGalleryBox);
            rowPanelActivities.Items.Add(new RibbonRowBreak() { Height = 30 });

            panelTools.Items.Add(rowPanelActivities);

            #region "RibbonButtons"

            var rowPanelToolButtons = new RibbonRowPanel() { Name = "ToolButtons", Id = "ToolButtons" };
            rowPanelToolButtons.Items.Add(new RibbonButton
            {
                Id = "OpenAllId",
                CommandHandler = _context != null ? _context.RbnOpenAll : null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("open_all", 1, 1),
                Name = "OpenAll",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "OpenAll",
                ToolTip = new RibbonToolTip
                {
                    Content = "Open all drawings",
                    IsHelpEnabled = false,
                    Title = "Open all drawings"
                }
            });
            rowPanelToolButtons.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelToolButtons.Items.Add(new RibbonButton
            {
                Id = "SettingsApplyId",
                CommandHandler = _context != null ? _context.RbnApply : null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("dwg_run", 1, 1),
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowImage = true,
                ShowText = true,
                Text = "Apply",
                ToolTip = new RibbonToolTip
                {
                    Content = "Apply settings to the active project",
                    IsHelpEnabled = false,
                    Title = "Apply"
                }
            });
            rowPanelToolButtons.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelToolButtons.Items.Add(new RibbonButton
            {
                Id = "LayoutsUploadId",
                CommandHandler = _context != null ? _context.RbnUpload : null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("dwg_upload", 1, 1),
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowImage = true,
                ShowText = true,
                Text = "Upload",
                ToolTip = new RibbonToolTip
                {
                    Content = "Upload data to the DB",
                    IsHelpEnabled = false,
                    Title = "Upload"
                }
            });
            rowPanelToolButtons.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelToolButtons.Items.Add(new RibbonButton
            {
                CommandHandler = _context != null ? _context.RbnExport : null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("dwg_export", 1, 1),
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowImage = true,
                ShowText = true,
                Text = "Export",
                ToolTip = new RibbonToolTip
                {
                    Content = "Save the active project to the project catalog",
                    IsHelpEnabled = false,
                    Title = "Export"
                }
            });
            panelTools.Items.Add(rowPanelToolButtons);
            panelTools.Items.Add(new RibbonSeparator() { SeparatorStyle = RibbonSeparatorStyle.Spacer });
            this.Panels.Add(new RibbonPanel { Source = panelTools });

            #endregion

            #endregion

            #region <Search>

            var panelSourceSearch = new RibbonPanelSource { Title = "Search", Name = "Search", Id = "Search" };
            panelSourceSearch.Items.Add(new RibbonButton
            {
                Text = "Search",
                Name = "Search",
                Id = "SearchId",
                CommandHandler = _context?.RbnOpenSearch,
                ShowText = true,
                LargeImage = rm.GetBitmapImage("find", 2, 1),
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                ShowImage = true,
                ToolTip = new RibbonToolTip
                {
                    Title = "Search",
                    IsHelpEnabled = false,
                    Content = "Search tools"
                }
            });
            this.Panels.Add(new RibbonPanel { Source = panelSourceSearch });

            #endregion

            #region <Queries>

            var panelSourceQueries = new RibbonPanelSource { Title = "", Name = "", Id = "Queries" };
#if INTEL
            panelSourceQueries = new RibbonPanelSource { Title = "Queries", Name = "Queries", Id = "Queries" };
            panelSourceQueries.Items.Add(new RibbonButton
            {
                Id = "QueriesId",
                Name = "Queries",
                Text = "Queries",
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("queries", 2, 1),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                ToolTip = new RibbonToolTip
                {
                    Content = "Queries",
                    IsHelpEnabled = false,
                    Title = "Queries"
                }
            });

            var rowPanelQueries = new RibbonRowPanel() { Name = "PanelQueries", Id = "PanelQueries" };
            rowPanelQueries.Items.Add(new RibbonButton
            {
                Id = "LayerQueriesId",
                CommandHandler = mainViewModel?.RbnLayerQueries,
                CommandParameter = null,
                Image = rm.GetBitmapImage("query_layers"),
                Name = "LayerQueries",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Layers",
                ToolTip = new RibbonToolTip
                {
                    Content = "Queries to layers",
                    IsHelpEnabled = false,
                    Title = "Queries to layers"
                }
            });
            rowPanelQueries.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelQueries.Items.Add(new RibbonButton
            {
                Id = "BayQueriesId",
                CommandHandler = mainViewModel?.RbnBayQueries,
                CommandParameter = null,
                Image = rm.GetBitmapImage("query_bays"),
                Name = "BayQueries",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Bays",
                ToolTip = new RibbonToolTip
                {
                    Content = "Queries to bays",
                    IsHelpEnabled = false,
                    Title = "Queries to bays"
                }
            });
            panelSourceQueries.Items.Add(rowPanelQueries);
#endif
            this.Panels.Add(new RibbonPanel { Source = panelSourceQueries });

            #endregion

            #region <Map>

            var panelSourceMap = new RibbonPanelSource { Title = "Map", Name = "Map", Id = "Map" };
            panelSourceMap.Items.Add(new RibbonButton
            {
                Id = "LoadMapId",
                CommandHandler = _context?.RbnMapView,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("map", 2, 1),
                Name = "Map",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Map",
                ToolTip = new RibbonToolTip
                {
                    Content = "Loading panel map",
                    IsHelpEnabled = false,
                    Title = "Load map"
                }
            });

            var rowPanelMap = new RibbonRowPanel() { Name = "PanelRowMap", Id = "PanelRowMap" };
            rowPanelMap.Items.Add(new RibbonButton
            {
                Id = "PointOnMapId",
                CommandHandler = _context?.RbnPointOnMap,
                CommandParameter = null,
                Image = rm.GetBitmapImage("map_point", 1, 1),
                Name = "PointOnMap",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Point",
                ToolTip = new RibbonToolTip
                {
                    Content = "Display a point on map",
                    IsHelpEnabled = false,
                    Title = "Point on Map"
                }
            });
            rowPanelMap.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelMap.Items.Add(new RibbonButton
            {
                Id = "FindOnMapId",
                CommandHandler = _context?.RbnFindOnMap,
                CommandParameter = null,
                Image = rm.GetBitmapImage("map_find", 1, 1),
                Name = "FindOnMap",
                Orientation = Orientation.Horizontal,
                ShowText = true,
                Size = RibbonItemSize.Standard,
                ShowImage = true,
                Text = "Find",
                ToolTip = new RibbonToolTip
                {
                    Content = "Find a object on map",
                    IsHelpEnabled = false,
                    Title = "Find on Map"
                }
            });
            rowPanelMap.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelMap.Items.Add(new RibbonButton
            {
                Id = "UCSChangeId",
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("dwg", 1, 1),
                Name = "UCSChange",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Ucs",
                ToolTip = new RibbonToolTip
                {
                    Content = "Change coordinate system",
                    IsHelpEnabled = false,
                    Title = "Change coordinate system"
                }
            });
            rowPanelMap.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelMap.Items.Add(new RibbonButton
            {
                Id = "ConvertToMarkersId",
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("dwg", 1, 1),
                Name = "ConvertToMarkers",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Convert",
                ToolTip = new RibbonToolTip
                {
                    Content = "Convert",
                    IsHelpEnabled = false,
                    Title = "Convert to markers"
                }
            });
            panelSourceMap.Items.Add(rowPanelMap);
            this.Panels.Add(new RibbonPanel { Source = panelSourceMap });

            #endregion

            #region <Commands>

            var panelSourceCommands = new RibbonPanelSource { Title = "Commands", Name = "Commands", Id = "Commands" };

            panelSourceCommands.Items.Add(new RibbonButton
            {
                Id = "RefreshId",
                CommandHandler = _context != null ? _context.RbnRefresh : null,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("dwg_refresh", 2, 1),
                Name = "Refresh",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Refresh",
                ToolTip = new RibbonToolTip
                {
                    Content = "Refresh and redraw",
                    IsHelpEnabled = false,
                    Title = "Refresh"
                }
            });

            panelSourceCommands.Items.Add(new RibbonButton
            {
                Id = "CleanId",
                CommandHandler = _context != null ? _context.RbnPurge : null,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("dwg_purge", 2, 1),
                Name = "Clean",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Clean",
                ToolTip = new RibbonToolTip
                {
                    Content = "Clean(purge) active drawing for not used objects",
                    IsHelpEnabled = false,
                    Title = "Clean"
                }
            });

            var rowPanelCommands = new RibbonRowPanel() { Name = "PanelCommands", Id = "PanelCommands" };
            rowPanelCommands.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("attach", 1, 1),
                Name = "ApplyToDrawing",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Apply",
                ToolTip = new RibbonToolTip
                {
                    Content = "Apply the analysis results as graphical annotations on the active drawing",
                    IsHelpEnabled = false,
                    Title = "Apply the analysis"
                }
            });
            rowPanelCommands.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelCommands.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("results_load", 1, 1),
                Name = "LoadResults",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Load",
                ToolTip = new RibbonToolTip
                {
                    Content = "Load results from others sources(formats)",
                    IsHelpEnabled = false,
                    Title = "Load Results"
                }
            });
            rowPanelCommands.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelCommands.Items.Add(new RibbonButton
            {
                Id = "RulerId",
                CommandHandler = null,
                CommandParameter = null,
                Image = Utils.GetAcadResourceImage("RCDATA_16_DIST"), //rm.GetBitmapImage("results_save", 1, 1),
                Name = "Ruler",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Ruler",
                ToolTip = new RibbonToolTip
                {
                    Content = "Ruler for polylines",
                    IsHelpEnabled = false,
                    Title = "Ruler for polylines"
                }
            });

            panelSourceCommands.Items.Add(rowPanelCommands);
            var panelCommands = new RibbonPanel { Source = panelSourceCommands };
            this.Panels.Add(panelCommands);

            #endregion

            #region <Blocks>

            var panelSourceBlocks = new RibbonPanelSource { Title = "Blocks", Name = "Blocks", Id = "Blocks" };
            panelSourceBlocks.Items.Add(new RibbonButton
            {
                Id = "BlocksId",
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("blocks", 2, 1),
                Name = "Blocks",
                Orientation = Orientation.Vertical,
                ShowText = true,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                Text = "Blocks",
                ToolTip = new RibbonToolTip
                {
                    Content = "Display panel of blocks",
                    IsHelpEnabled = false,
                    Title = "Blocks"
                }
            });

            var rowPanelBlocks = new RibbonRowPanel() { Name = "PanelBlocks", Id = "PanelBlocks" };
            rowPanelBlocks.Items.Add(new RibbonButton
            {
                Id = "CopyAsBlockId",
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("copy"),
                Name = "CopyAs",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Copy",
                ToolTip = new RibbonToolTip
                {
                    Content = "Copy current dwg as block",
                    IsHelpEnabled = false,
                    Title = "Copy current dwg as block"
                }
            });
            rowPanelBlocks.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelBlocks.Items.Add(new RibbonButton
            {
                Id = "PasteAsBlockId",
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("paste"),
                Name = "PasteAs",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Paste",
                ToolTip = new RibbonToolTip
                {
                    Content = "Paste block at base point",
                    IsHelpEnabled = false,
                    Title = "Paste as block"
                }
            });

            panelSourceBlocks.Items.Add(rowPanelBlocks);
            var panelBlocks = new RibbonPanel { Source = panelSourceBlocks };
            this.Panels.Add(panelBlocks);

            #endregion

            #region <Fibers>
#if PARTNER
            var panelSourceFibers = new RibbonPanelSource { Title = "", Name = "", Id = "Fibers" };
            panelSourceFibers = new RibbonPanelSource { Title = "Fibers", Name = "Fibers", Id = "Fibers" };
            panelSourceFibers.Items.Add(new RibbonButton
            {
                Id = "FibersId",
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("fibers", 2, 1),
                Name = "Fibers",
                Orientation = Orientation.Vertical,
                ShowText = true,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                Text = "Fibers",
                ToolTip = new RibbonToolTip
                {
                    Content = "Display panel of fibers",
                    IsHelpEnabled = false,
                    Title = "Fibers"
                }
            });

            var rowPanelFibers = new RibbonRowPanel() { Name = "PanelFibers", Id = "PanelFibers" };
            rowPanelFibers.Items.Add(new RibbonButton
            {
                Id = "CableId",
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("cables", 1, 1),
                Name = "Cable",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Cable",
                ToolTip = new RibbonToolTip
                {
                    Content = "Open cable panel",
                    IsHelpEnabled = false,
                    Title = "Open cable panel"
                }
            });

            rowPanelFibers.Items.Add(new RibbonRowBreak() { Height = 10 });
            rowPanelFibers.Items.Add(new RibbonButton
            {
                Id = "CabinetId",
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("cabinets", 1, 1),
                Name = "Cabinet",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Cabinet",
                ToolTip = new RibbonToolTip
                {
                    Content = "Open cabinet panel",
                    IsHelpEnabled = false,
                    Title = "Open cabinet panel"
                }
            });

            rowPanelFibers.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelFibers.Items.Add(new RibbonButton
            {
                Id = "ClosureId",
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("closures", 1, 1),
                Name = "Closure",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Closure",
                ToolTip = new RibbonToolTip
                {
                    Content = "Open closure panel",
                    IsHelpEnabled = false,
                    Title = "Open closure panel"
                }
            });

            rowPanelFibers.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelFibers.Items.Add(new RibbonButton
            {
                Id = "ClosureConnectId",
                CommandHandler = null,
                CommandParameter = null,
                Image = rm.GetBitmapImage("closures", 1, 1),
                Name = "ClosureConnect",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "Connect",
                ToolTip = new RibbonToolTip
                {
                    Content = "Open closure connect panel",
                    IsHelpEnabled = false,
                    Title = "Open closure connect panel"
                }
            });

            panelSourceFibers.Items.Add(rowPanelFibers);
            var panelFibers = new RibbonPanel { Source = panelSourceFibers };
            this.Panels.Add(panelFibers);
#endif
            #endregion

            #region <Plot>

            var panelSourcePlot = new RibbonPanelSource { Title = "Plot", Name = "Plot", Id = "Plot" };
            panelSourcePlot.Items.Add(new RibbonButton
            {
                Id = "PlotId",
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = Utils.GetAcadResourceImage("RCDATA_32_PUBL"),
                Name = "Plot",
                Orientation = Orientation.Vertical,
                ShowText = true,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                Text = "Plot",
                ToolTip = new RibbonToolTip
                {
                    Content = "Display plot panel",
                    IsHelpEnabled = false,
                    Title = "Plot"
                }
            });

            var rowPanelPlot = new RibbonRowPanel() { Name = "PanelPlot", Id = "PanelPlot" };
            rowPanelPlot.Items.Add(new RibbonButton
            {
                Id = "PlotDwgId",
                CommandHandler = null,
                CommandParameter = null,
                Image = Utils.GetAcadResourceImage("RCDATA_16_PUBL"),
                Name = "PlotDwg",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "PlotDwg",
                ToolTip = new RibbonToolTip
                {
                    Content = "Open PlotToDwg panel",
                    IsHelpEnabled = false,
                    Title = "Open PlotToDwg panel"
                }
            });
            rowPanelPlot.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelPlot.Items.Add(new RibbonButton
            {
                Id = "PlotToPdfId",
                CommandHandler = null,
                CommandParameter = null,
                Image = Utils.GetAcadResourceImage("RCDATA_16_PUBLISH_PDF_OPTIONS"),
                Name = "PlotToPdf",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "PlotToPdf",
                ToolTip = new RibbonToolTip
                {
                    Content = "Open PlotToPdf panel",
                    IsHelpEnabled = false,
                    Title = "Open PlotToPdf panel"
                }
            });
            rowPanelPlot.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanelPlot.Items.Add(new RibbonButton
            {
                Id = "PlotToPltId",
                CommandHandler = null,
                CommandParameter = null,
                Image = Utils.GetAcadResourceImage("RCDATA_16_PUBLISH_OPTIONS"),
                Name = "PlotToPlt",
                Orientation = Orientation.Horizontal,
                Size = RibbonItemSize.Standard,
                ShowText = true,
                ShowImage = true,
                Text = "PlotToPlt",
                ToolTip = new RibbonToolTip
                {
                    Content = "Open PlotToPlt panel",
                    IsHelpEnabled = false,
                    Title = "Open PlotToPlt panel"
                }
            });

            panelSourcePlot.Items.Add(rowPanelPlot);
            var panelPlot = new RibbonPanel { Source = panelSourcePlot };
            this.Panels.Add(panelPlot);

            #endregion

            #region <Settings,Help,Notify>

            var panelSourceSettings = new RibbonPanelSource { Title = "Settings", Name = "Settings", Id = "Settings" };

            panelSourceSettings.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("dwg_options", 2, 1),
                Name = "Config",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Config",
                ToolTip = new RibbonToolTip
                {
                    Content = "User preferences and admin settings",
                    IsHelpEnabled = false,
                    Title = "Configuration"
                }
            });

            panelSourceSettings.Items.Add(new RibbonButton
            {
                Id = "HelpCommandId",
                CommandHandler = _context != null ? _context.RbnHelpCommand : null,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("dwg_help", 2, 1),
                Name = "Help",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Help",
                ToolTip = new RibbonToolTip
                {
                    Content = "Usage manuals and ‘About’",
                    IsHelpEnabled = false,
                    Title = "Help"
                }
            });

            var panelSettings = new RibbonPanel { Source = panelSourceSettings };
            this.Panels.Add(panelSettings);

            var panelSourceNotification = new RibbonPanelSource { Title = "Notification" };

            _notificationsButton = new RibbonButton
            {
                Id = "NotificationsId",
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = rm.GetBitmapImage("note_ready", 2, 1),
                Name = "Notifications",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Working...",
                ToolTip = new RibbonToolTip
                {
                    Content = "Indicator of tasks",
                    IsHelpEnabled = false,
                    Title = "Notifications"
                }
            };
            panelSourceNotification.Items.Add(_notificationsButton);

            var panel4 = new RibbonPanel { Source = panelSourceNotification };
            this.Panels.Add(panel4);

            #endregion

            //if (mainPanel.CurrentUserSetting != null && mainPanel.CurrentUserSetting.IsActive)
            this.IsActive = true;

            //if (mainPanel != null)
            //    this.Activated += (Sender, Args) => mainPanel.OnDocumentActivated(null, null);
        }

        public void LoadControlData()
        {
            var ribbonControl = RibbonControl ?? ComponentManager.Ribbon;
            ribbonControl.DataContext = _context;

            //RibbonTab ribbonTab = ribbonControl.Tabs
            //    .FirstOrDefault(x => x.Id == Plugin.Settings.Name);

            if (_context == null) return;

            RibbonPanel explorerPanel = this.Panels.Single(x => x.Source.Id == "Explorer");

            RibbonButton btnProjectExplorer = (RibbonButton)explorerPanel.FindItem("RbExplorerId");
            btnProjectExplorer.CommandHandlerBinding = new Binding("RbnOpenExplorer") { Source = _context };

            RibbonCombo layoutComboBox = (RibbonCombo)explorerPanel.FindItem("RbnLayoutsId");

            layoutComboBox.DropDownOpened += OnlayoutComboBoxDropDownOpened;
            layoutComboBox.DropDownClosed += OnlayoutComboBoxDropDownClosed;
            //layoutComboBox.CurrentChanged += viewModel.OnlayoutComboBoxCurrentChanged;

            if (_context.LayoutItemButtons != null && _context.LayoutItemButtons.Count > 0)
            {
                layoutComboBox.ItemsBinding = new Binding("LayoutItemButtons") { Source = _context }; //RibbonLayoutItems
                layoutComboBox.Items.Clear();
                layoutComboBox.Items.AddRange(_context.LayoutItemButtons);
                //layoutComboBox.Current = viewModel.LayoutItemButtons.FirstOrDefault();
                //layoutComboBox.CurrentBinding = new Binding("CurrentRibbonLayout") { Source = viewModel };

                var settingsGallery = (RibbonCombo)explorerPanel.FindItem("SettingsGalleryId");
                settingsGallery.Items.Clear();

                var layout = _context.LayoutItems.FirstOrDefault(x => x.LayoutName == _context.CurrentRibbonLayout.Text);

                if (layout != null && layout.WorkItems != null && layout.WorkItems.Any())
                    settingsGallery.Items.AddRange(layout.WorkItems.Select(x => new RibbonButton() { Text = x.Work }));
            }

            RibbonButton btn = (RibbonButton)this.Panels[PanelNames.ProjetExplorer].FindItem("OpenAllId");
            btn.CommandHandler = _context.RbnOpenAll;

            btn = (RibbonButton)this.Panels[PanelNames.ProjetExplorer].FindItem("LayoutsUploadId");
            btn.CommandHandler = _context.RbnUpload;

            btn = (RibbonButton)this.Panels[PanelNames.Search].FindItem("SearchId");
            btn.CommandHandler = _context.RbnOpenSearch;

            btn = (RibbonButton)this.Panels[PanelNames.Queries].FindItem("LayerQueriesId");
            if (btn != null) btn.CommandHandler = _context.RbnLayerQueries;

            btn = (RibbonButton)this.Panels[PanelNames.Queries].FindItem("BayQueriesId");
            if (btn != null) btn.CommandHandler = _context.RbnBayQueries;

            btn = (RibbonButton)this.Panels[PanelNames.Map].FindItem("LoadMapId");
            btn.CommandHandler = _context.RbnMapView;

            btn = (RibbonButton)this.Panels[PanelNames.Map].FindItem("PointOnMapId");
            btn.CommandHandler = _context.RbnPointOnMap;

            btn = (RibbonButton)this.Panels[PanelNames.Map].FindItem("FindOnMapId");
            btn.CommandHandler = _context.RbnFindOnMap;

            btn = (RibbonButton)this.Panels[PanelNames.Map].FindItem("UCSChangeId");
            btn.CommandHandler = _context.RbnUcsChange;

            btn = (RibbonButton)this.Panels[PanelNames.Map].FindItem("ConvertToMarkersId");
            btn.CommandHandler = _context.RbnConvertToMarkers;

            btn = (RibbonButton)this.Panels[PanelNames.Blocks].FindItem("CopyAsBlockId");
            btn.CommandHandler = _context.RbnCopyAsBlock;

            btn = (RibbonButton)this.Panels[PanelNames.Blocks].FindItem("PasteAsBlockId");
            btn.CommandHandler = _context.RbnPasteAsBlock;

#if PARTNER
            btn = (RibbonButton)this.Panels[PanelNames.Fibers].FindItem("CableId");
            btn.CommandHandler = _context.RbnCable;

            btn = (RibbonButton)this.Panels[PanelNames.Fibers].FindItem("CabinetId");
            btn.CommandHandler = _context.RbnCabinet;

            btn = (RibbonButton)this.Panels[PanelNames.Fibers].FindItem("ClosureId");
            btn.CommandHandler = _context.RbnClosure;

            btn = (RibbonButton)this.Panels[PanelNames.Fibers].FindItem("ClosureConnectId");
            btn.CommandHandler = _context.RbnClosureConnect;
#endif

            btn = (RibbonButton)this.Panels[PanelNames.Commands].FindItem("RefreshId");
            btn.CommandHandler = _context.RbnRefresh;

            btn = (RibbonButton)this.Panels[PanelNames.Commands].FindItem("CleanId");
            btn.CommandHandler = _context.RbnPurge;

            btn = (RibbonButton)this.Panels[PanelNames.Commands].FindItem("RulerId");
            btn.CommandHandler = _context.RbnRuler;

            btn = (RibbonButton)this.Panels[PanelNames.Plot].FindItem("PlotDwgId");
            btn.CommandHandler = _context.RbnPlotW;

            btn = (RibbonButton)this.Panels[PanelNames.Settings].FindItem("HelpCommandId");

            //var myNotificationAwareObject = new InteractionRequestViewModel();
            //var binding = new Binding("ConfirmationRequest") { Source = myNotificationAwareObject, Mode = BindingMode.OneWay };
            //var trigger = new InteractionRequestTrigger() { SourceObject = binding };
            //InteractionRequestedEventArgs eventArgs = null;
            //var actionListener = new ActionListener((e) => { eventArgs = (InteractionRequestedEventArgs)e; });
            //trigger.Actions.Add(actionListener);
            //trigger.Attach(this.RibbonControl.);
        }

        private void OnlayoutComboBoxDropDownClosed(object sender, System.EventArgs e)
        {
            ((RibbonCombo)sender).Tag = null;
        }

        private void OnlayoutComboBoxDropDownOpened(object sender, System.EventArgs e)
        {
            ((RibbonCombo)sender).Tag = "OnDropDownOpened";
        }

        private void OnNotifyMessage(NotifyArgs args)
        {
            var ribbonControl = RibbonControl ?? ComponentManager.Ribbon;
            if (ribbonControl.IsVisible)
            {
                Plugin.RunOnUIThreadAsync(() =>
                {
                    //Mouse.OverrideCursor = args.Status == NotifyStatus.Loading ? Cursors.Wait : null;

                    ribbonControl.Focus();

                    //var resourceClass = ConfigurationManager.AppSettings["resourceClass"];
                    //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

                    if (_notificationsButton == null) return;

                    _notificationsButton.Text = args.Text ?? "Ready";

                    var resourceManager = Intellidesk.Resources.Properties.Resources.ResourceManager;
                    var resourceName = NotifyAttribute.Get(args.Status).ToLower();
                    BitmapImage resourceImage;

                    if (!_resourceImages.ContainsKey(resourceName))
                    {
                        resourceImage = resourceManager.GetBitmapImage("note_" + resourceName, 2, 1);
                        if (resourceImage == null)
                        {
                            resourceName = "note_ready";
                            resourceImage = resourceImage ?? resourceManager.GetBitmapImage(resourceName, 2, 1);
                        }
                        if (!_resourceImages.ContainsKey(resourceName))
                            _resourceImages.Add(resourceName, resourceImage);
                    }
                    else
                    {
                        resourceImage = _resourceImages[resourceName];
                    }

                    _notificationsButton.LargeImage = resourceImage ?? resourceManager.GetBitmapImage("note_ready", 2, 1);
                    _notificationsButton.ToolTip = args.CommanName +
                        (string.IsNullOrEmpty(args.CommanName) ? "" : ": ") + args.Tooltip;
                });
            }
            else
            {
                DisplayNotifyMessage(args.Text);
            }
        }

        private void DisplayNotifyMessage(string notifyMessage)
        {
            if (acadApp.DocumentManager.MdiActiveDocument != null)
            {
                notifyMessage += notifyMessage.ToLower().Contains("working")
                    || notifyMessage.ToLower().Contains("loading")
                    ? "..." : "";

                var ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage(_pluginSettings.Prompt + "plugin " + notifyMessage.ToLower() + "\n");
            }
        }
    }
}