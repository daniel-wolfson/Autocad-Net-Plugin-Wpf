using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Autodesk.Windows;

using Intellidesk.AcadNet.Data.Models;
using Intellidesk.AcadNet.Resources.Properties;
using Intellidesk.AcadNet.ViewModels;
using Intellidesk.Infrastructure;

using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;
using Intellidesk.Infrastructure.Core;
using Intellidesk.Infrastructure.Enums;
using Intellidesk.Infrastructure.Extensions;

namespace Intellidesk.AcadNet
{
    public static class RibbonTabBuilder
    {
        static RibbonTabBuilder()
        {
        }
        //private static ICommand _rbnOpenProjectExplorer;
        //public static Lazy<ICommand> RbnOpenProjectExplorer
        //{
        //    get
        //    {
        //        var buildService = PluginBootstrapper.PluginContainer.Resolve<IUIBuildService>();
        //        var result = _rbnOpenProjectExplorer ??
        //                     (_rbnOpenProjectExplorer = buildService.ProjectExplorerViewModel.RbnOpenProjectExplorer);
        //        return buildService.ProjectExplorerViewModel.RbnOpenProjectExplorer;
        //    }
        //}

        /// <summary> Creare RibbonTab </summary>
        public static RibbonTab Load(this RibbonControl ribbonControl, string ribbonTabName,
            ProjectExplorerViewModel mainViewModel = null,
            InteractionRequestViewModel interactionRequestViewModel = null)
        //Action<object, DocumentCollectionEventArgs> onTabActivated = null)
        {
            // Reference to project resources
            var resourceManager = new ResourceManager("Intellidesk.AcadNet.Resources.Properties.Images",
                Assembly.GetAssembly(typeof(Images)));
            //var resourceManager = new ResourceManager("IntelliDesk.AcadNet.Properties.Resources", typeof(Resources).Assembly);

            //var NetToolsResourceManager = UIManager.GetResourceManager();
            //ribbonControl.DataContext = ProjectExplorerViewModel;

            var ribbonTab = new RibbonTab { Title = ribbonTabName, Id = "ID_" + ribbonTabName };
            ribbonControl.Tabs.Add(ribbonTab);

            #region "Project's Catalog"

            // create panel source and create panel
            var panelSource1 = new RibbonPanelSource { Title = "Project's Catalog" };
            var btnProjectExplorer = new RibbonButton
            {
                Text = "Project\nExplorer",
                Name = "ProjectExplorer",
                Id = "RbProjectExplorerId",
                CommandHandler = mainViewModel != null ? mainViewModel.RbnOpenProjectExplorer : null,
                ShowText = true,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("find"), 2, 1),
                Size = RibbonItemSize.Large,
                Orientation = Orientation.Vertical,
                ShowImage = true,
                ToolTip = new RibbonToolTip
                {
                    Title = "Layout Explorer",
                    IsHelpEnabled = false,
                    Content = "Browse, load and manage layouts stored in the library"
                }
            };

            //var buildService = PluginBootstrapper.PluginContainer.Resolve<IUIBuildService>();
            ////btnProjectExplorer.CommandHandler = RbnOpenProjectExplorer;
            //btnProjectExplorer.CommandHandlerBinding = new Binding("RbnOpenProjectExplorer") 
            //{ Source = new Lazy<ProjectExplorerViewModel>(() => buildService.ProjectExplorerViewModel) };

            panelSource1.Items.Add(btnProjectExplorer);
            var panel1 = new RibbonPanel { Source = panelSource1 };
            ribbonTab.Panels.Add(panel1);

            // Create a Command Item that the Dialog Launcher can use, for this test it is just a place holder.
            var bitmapImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("prop_arrow"));
            var dialogLauncherButton = new RibbonButton
            {
                Name = "Options",
                Image = bitmapImage,
                LargeImage = bitmapImage
            };
            panelSource1.DialogLauncher = dialogLauncherButton;

            var rowPanel2 = new RibbonRowPanel() { Name = "Panel2", Id = "Panel2" };
            // Add new panel into our tab
            var rlLayout = new RibbonLabel() { Text = "Active Project:", Width = 95 };
            var rlConfig = new RibbonLabel() { Text = "Active Settings:", Width = 95 };

            #region "LoadLayouts"

            var layoutComboBox = new RibbonCombo
            {
                Name = "Project Gallery",
                Id = "RbnProjectsId",
                Width = 200,
                ToolTip = new RibbonToolTip
                {
                    Title = "Active Project",
                    IsHelpEnabled = false,
                    Content = "Select and switch to one of the drawings currently open in AutoCAD"
                }
            };

            //layoutComboBox.DropDownOpened += OnlayoutComboBoxDropDownOpened;
            //layoutComboBox.DropDownClosed += OnlayoutComboBoxDropDownClosed;
            //layoutComboBox.CurrentChanged += mainViewModel.OnlayoutComboBoxCurrentChanged;

            //var bind = new Binding("RibbonLayoutItems") { Source = mainViewModel };
            //layoutComboBox.ItemsBinding = bind;
            //layoutComboBox.Current = mainViewModel.RibbonLayoutItems.FirstOrDefault(x => x.Id == "none");
            //bind = new Binding("CurrentRibbonLayout") { Source = mainViewModel }; //ComponentManager.Ribbon.DataContext
            //layoutComboBox.CurrentBinding = bind;

            #endregion

            #region "LoadConfigs"

            var configGalleryBox = new RibbonCombo
            {
                Name = "Config Gallery",
                Id = "ConfigGalleryId",
                Width = 200,
                ToolTip = new RibbonToolTip
                {
                    Title = "Active Settings",
                    IsHelpEnabled = false,
                    Content = "The configuration set currently being used for layout catalog options"
                }
            };

            RibbonButton rb; RibbonToolTip tt;
            var configs = mainViewModel != null ? mainViewModel.ConfigItems.ToList() : new List<Config>();
            if (configs.Count != 0)
            {
                foreach (var config in configs)
                {
                    rb = new RibbonButton
                    {
                        Name = "ConfigItem_" + config.ConfigSetName,
                        CommandParameter = config,
                        CommandHandler = mainViewModel.RbnOpenConfigCommand,
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
                                frameLayerTypes.Add("  FRAME_TYPE_ID=" + conf.Key.ToString().Trim() + "," + String.Join(",", layerTypes));
                        });

                    var attrs = String.Join(", ", config.LayoutOptions
                        .Where(x => x.ParameterName == "TOOL_NAME_ATTRIBUTE").Select(x1 => x1.Value).ToArray());

                    var toolTipString = String.Format("LAYER_TYPES :\n{0};\n\nTOOL_NAME_ATTRIBUTES :\n  {1}", String.Join(";\n ", frameLayerTypes), attrs);
                    tt = new RibbonToolTip
                    {
                        Title = config.ConfigSetName,
                        IsHelpEnabled = false,
                        Command = "Command: PARTNERCONFIG ",
                        Content = toolTipString
                    };
                    rb.ToolTip = tt;

                    configGalleryBox.Items.Add(rb);

                    if (config.ConfigSetName == mainViewModel.CurrentConfig.ConfigSetName)
                        configGalleryBox.Current = rb;
                }
            }
            else
            {
                tt = new RibbonToolTip { IsHelpEnabled = false };
                rb = new RibbonButton
                {
                    Name = "ConfigItem_",
                    CommandParameter = tt.Command = "",
                    CommandHandler = null, // custom command handler
                    Orientation = Orientation.Horizontal,
                    Size = RibbonItemSize.Standard,
                    Height = 28,
                    Text = tt.Title = "None", //Header button and 
                    ShowImage = false,
                    ShowText = true,
                    Tag = ""
                };
                tt.Content = "Configurations not found";
                rb.ToolTip = tt;
                configGalleryBox.Items.Add(rb);
            }

            #endregion

            rowPanel2.Items.Add(rlLayout);
            rowPanel2.Items.Add(layoutComboBox);
            rowPanel2.Items.Add(new RibbonRowBreak() { Height = 30 });
            rowPanel2.Items.Add(rlConfig);
            rowPanel2.Items.Add(configGalleryBox);

            panelSource1.Items.Add(rowPanel2);
            panelSource1.Items.Add(new RibbonSeparator() { SeparatorStyle = RibbonSeparatorStyle.Spacer });

            #region "RibbonButtons"

            var rbApply = new RibbonButton
            {
                CommandHandler = mainViewModel != null ? mainViewModel.RbnApply : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("dwg_run"),2,1),
                //Image = LoadImage("upload_32"), //Images.GetBitmap((Bitmap)_resourceManager.GetObject("upload_32")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Apply",
                ToolTip = new RibbonToolTip
                {
                    Content = "Apply settings to the active project",
                    IsHelpEnabled = false,
                    Title = "Apply"
                }
            };
            panelSource1.Items.Add(rbApply);

            var rbUpload = new RibbonButton
            {
                CommandHandler = mainViewModel != null ? mainViewModel.RbnUpload : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("dwg_upload"),2,1),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Upload",
                ToolTip = new RibbonToolTip
                {
                    Content = "Upload data to the DB",
                    IsHelpEnabled = false,
                    Title = "Upload"
                }
            };
            panelSource1.Items.Add(rbUpload);

            var rbExport = new RibbonButton
            {
                CommandHandler = mainViewModel != null ? mainViewModel.RbnExport : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("dwg_export"),2,1), //Image = LoadImage("icon_32")
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Export",
                ToolTip = new RibbonToolTip
                {
                    Content = "Save the active project to the project catalog",
                    IsHelpEnabled = false,
                    Title = "Export"
                }
            };

            #endregion

            panelSource1.Items.Add(rbExport);

            #endregion

            #region "Commands"

            var panelSourceCommands = new RibbonPanelSource { Title = "Commands" };

            panelSourceCommands.Items.Add(new RibbonButton
            {
                Id = "LoadFromMapId",
                CommandHandler = mainViewModel != null ? mainViewModel.RbnMapInfoGetData : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("map_fetch")),
                Name = "LoadFromMap",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Load\nfrom Map",
                ToolTip = new RibbonToolTip
                {
                    Content = "Loading data about object from map and open his dwg",
                    IsHelpEnabled = false,
                    Title = "Load from map"
                }
            });

            panelSourceCommands.Items.Add(new RibbonButton
            {
                Id = "SentToMapId",
                CommandHandler = mainViewModel != null ? mainViewModel.RbnSentToMap : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("map_view")),
                Name = "SentToMap",
                Orientation = Orientation.Vertical,
                ShowText = true,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                Text = "Sent\nto Map",
                ToolTip = new RibbonToolTip
                {
                    Content = "Sent a point to map",
                    IsHelpEnabled = false,
                    Title = "Sent to Map"
                }
            });

            panelSourceCommands.Items.Add(new RibbonButton
            {
                Id = "RefreshId",
                CommandHandler = mainViewModel != null ? mainViewModel.RbnRefresh : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("dwg_refresh"), 2, 1),
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
                CommandHandler = mainViewModel != null ? mainViewModel.RbnPurge : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("dwg_purge"), 2, 1),
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

            var panelCommands = new RibbonPanel { Source = panelSourceCommands };
            ribbonTab.Panels.Add(panelCommands);

            #endregion

            #region "Actions"

            var panelSourceActions = new RibbonPanelSource { Title = "Actions" };

            panelSourceActions.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("attach"),2,1),
                Name = "ApplyToDrawing",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Apply\nto drawing",
                ToolTip = new RibbonToolTip
                {
                    Content = "Apply the analysis results as graphical annotations on the active drawing",
                    IsHelpEnabled = false,
                    Title = "Apply the analysis"
                }
            });

            panelSourceActions.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("results_load"),2,1),
                Name = "LoadResults",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Load\nResults",
                ToolTip = new RibbonToolTip
                {
                    Content = "Load results from others sources(formats)",
                    IsHelpEnabled = false,
                    Title = "Load Results"
                }
            });

            panelSourceActions.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("results_save"),2,1),
                Name = "SaveResults",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Save\nResults",
                ToolTip = new RibbonToolTip
                {
                    Content = "Save results to others formats",
                    IsHelpEnabled = false,
                    Title = "Save results"
                }
            });

            var panelActions = new RibbonPanel { Source = panelSourceActions };
            ribbonTab.Panels.Add(panelActions);

            #endregion

            #region "Settings,Help,Notify"

            var panelSourceSettings = new RibbonPanelSource { Title = "Settings" };

            panelSourceSettings.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("dwg_options"),2,1),
                Name = "Config",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Configuration",
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
                CommandHandler = mainViewModel != null ? mainViewModel.RbnHelpCommand : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("dwg_help"),2,1),
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
            ribbonTab.Panels.Add(panelSettings);

            var panelSourceNotification = new RibbonPanelSource { Title = "Notification" };

            IEventAggregator aggregator = PluginBootstrapper.PluginContainer.Resolve<IEventAggregator>();
            aggregator.GetEvent<NotifyMessageEvent>().Unsubscribe(NotifyUpdateMessage);
            aggregator.GetEvent<NotifyMessageEvent>().Subscribe(NotifyUpdateMessage);
            aggregator.GetEvent<NotifyMessageStringEvent>().Subscribe(NotifyUpdateMessage);

            panelSourceNotification.Items.Add(new RibbonButton
            {
                Id = "NotificationsId",
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("note_ready"), 2, 1),
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
            });

            var panel4 = new RibbonPanel { Source = panelSourceNotification };
            ribbonTab.Panels.Add(panel4);

            #endregion

            //if (mainViewModel.CurrentUserSetting != null && mainViewModel.CurrentUserSetting.IsActive)
            ribbonTab.IsActive = true;

            //if (mainViewModel != null)
            //    ribbonTab.Activated += (sender, args) => mainViewModel.OnDocumentActivated(null, null);

            return ribbonTab;
        }

        private static ICommand LazyCommandHandler()
        {
            throw new NotImplementedException();
        }

        public static void TabNotifySendMessage(this RibbonControl ribbonControl, string notifyMessage = null)
        {
            var resourceManager = new ResourceManager("Intellidesk.AcadNet.Resources.Properties.Images",
                Assembly.GetAssembly(typeof(Images)));
            var ribbonTab = ribbonControl.Tabs.First(x => x.Id == "ID_Partner");

            var btn = ribbonTab.Panels[4].Source.Items[0];
            btn.Text = notifyMessage ?? "Ready";
            btn.LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject(
                    notifyMessage != null ? "note_ready" : "note_success"), 2, 1);
        }

        public static RibbonTab LoadData(this RibbonControl ribbonControl, ProjectExplorerViewModel viewModel)
        {
            ribbonControl.DataContext = viewModel;
            var ribbonTab = ribbonControl.Tabs.First(x => x.Id == "ID_Partner");

            var btnRefresh = (RibbonButton)ribbonTab.Panels[1].FindItem("RefreshId");
            btnRefresh.CommandHandler = viewModel.RbnRefresh;

            var btnClean = (RibbonButton)ribbonTab.Panels[1].FindItem("CleanId");
            btnClean.CommandHandler = viewModel.RbnPurge;

            var btnLoadAsmade = (RibbonButton)ribbonTab.Panels[1].FindItem("LoadFromMapId");
            btnLoadAsmade.CommandHandler = viewModel.RbnMapInfoGetData;

            var btnSentToMap = (RibbonButton)ribbonTab.Panels[1].FindItem("SentToMapId");
            btnSentToMap.CommandHandler = viewModel.RbnSentToMap;

            var btnHelpCommand = (RibbonButton)ribbonTab.Panels[3].FindItem("HelpCommandId");
            btnHelpCommand.CommandHandler = viewModel.RbnHelpCommand;

            var btnProjectExplorer = (RibbonButton)ribbonTab.Panels[0].FindItem("RbProjectExplorerId");
            //btnOpenProjectExplorer.CommandHandlerBinding = new Binding("RbnSampleCommand") { Source = mainViewModel };
            btnProjectExplorer.CommandHandlerBinding =
                new Binding("RbnOpenProjectExplorer") { Source = viewModel };
            //btnProjectExplorer.CommandHandler = mainViewModel.RbnOpenProjectExplorer;

            var layoutComboBox = (RibbonCombo)ribbonTab.Panels[0].FindItem("RbnProjectsId");
            layoutComboBox.DropDownOpened += OnlayoutComboBoxDropDownOpened;
            layoutComboBox.DropDownClosed += OnlayoutComboBoxDropDownClosed;
            layoutComboBox.CurrentChanged += viewModel.OnlayoutComboBoxCurrentChanged;

            Binding bind;
            if (viewModel.RibbonLayoutItems != null && viewModel.RibbonLayoutItems.Count > 0)
            {
                bind = new Binding("RibbonLayoutItems") { Source = viewModel };
                layoutComboBox.ItemsBinding = bind;
                layoutComboBox.Current = viewModel.RibbonLayoutItems.FirstOrDefault(x => x.Id == "none");
            }

            bind = new Binding("CurrentRibbonLayout") { Source = viewModel }; //ComponentManager.Ribbon.DataContext
            layoutComboBox.CurrentBinding = bind;

            //var myNotificationAwareObject = new InteractionRequestViewModel();
            //var binding = new Binding("ConfirmationRequest") { Source = myNotificationAwareObject, Mode = BindingMode.OneWay };
            //var trigger = new InteractionRequestTrigger() { SourceObject = binding };

            //InteractionRequestedEventArgs eventArgs = null;
            //var actionListener = new ActionListener((e) => { eventArgs = (InteractionRequestedEventArgs)e; });
            //trigger.Actions.Add(actionListener);
            //trigger.Attach(ribbonTab.RibbonControl.);

            return ribbonTab;
        }

        private static bool MyMethodCall()
        {
            System.Windows.MessageBox.Show("ok");
            return true;
        }

        /// <summary>is contains Tab </summary>
        public static bool ContainsTab(this RibbonControl control, string ribbonTabName)
        {
            return control != null && control.Tabs.Any(tab => tab.Id.Equals("ID_" + ribbonTabName) && tab.Title.Equals(ribbonTabName));
        }

        /// <summary>Find Tab </summary>
        public static RibbonTab FindTab(this RibbonControl control, string ribbonTabName)
        {
            return control.Tabs.FirstOrDefault(tab => tab.Id.Equals("ID_" + ribbonTabName) && tab.Title.Equals(ribbonTabName));
        }

        /// <summary> Is Ribbon Current </summary>
        public static bool IsTabCurrent(this RibbonControl control, string ribbonTabName)
        {
            return control.Tabs.Any(tab => tab.Id.Equals("ID_" + ribbonTabName) && tab.Title.Equals(ribbonTabName) && tab.IsActive);
        }

        ///// <summary> Occuring at layoutComboBoxDropDown on event Opened </summary>
        public static void OnlayoutComboBoxDropDownOpened(object sender, EventArgs e)
        {
            ((RibbonCombo)sender).Tag = "OnDropDownOpened";
        }

        /// <summary> Occuring at layoutComboBoxDropDown on event Closed </summary>
        public static void OnlayoutComboBoxDropDownClosed(object sender, EventArgs e)
        {
            ((RibbonCombo)sender).Tag = null;
        }

        /// <summary> Creare RibbonTab </summary>
        public static RibbonTab CreateRibbonTab(string ribbonTabName, object viewModel)
        {
            //ResourceManager = new ResourceManager("AcadNet.Properties.Resources", typeof(Resources).Assembly);
            //CurrentRibbonTabName = ribbonTabName;

            //ComponentManager.Ribbon contains all others controls
            //RibbonControl = ComponentManager.Ribbon;

            //RibbonControl.DataContext = viewModel;
            var ribbonTab = new RibbonTab { Title = "Partner", Id = "ID_Partner" };
            ComponentManager.Ribbon.Tabs.Add(ribbonTab);
            return ribbonTab;
        }

        public static void NotifyUpdateMessage(string notifyMessage)
        {
            NotifyUpdateMessage(new NotifyArgs(NotifyImageStatus.Info, notifyMessage));
        }

        public static void NotifyUpdateMessage(NotifyArgs args)
        {
            var ribbonControl = ComponentManager.Ribbon;
            var resourceManager = new ResourceManager("Intellidesk.AcadNet.Resources.Properties.Images",
                Assembly.GetAssembly(typeof(Images)));

            var ribbonTab = ribbonControl.Tabs.First(x => x.Id == "ID_Partner");
            var btn = ribbonTab.Panels[4].Source.Items[0];
            btn.Text = args.Text;

            var resourceName = NotifyExtension.Get(args.Status).ToLower();
            var resourceImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("note_" + resourceName), 2, 1);

            btn.LargeImage = resourceImage ?? ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("note_ready"), 2, 1);
            btn.ToolTip = args.CommanName + (string.IsNullOrEmpty(args.CommanName) ? "" : ": ") + args.Tooltip;
        }
    }
}
