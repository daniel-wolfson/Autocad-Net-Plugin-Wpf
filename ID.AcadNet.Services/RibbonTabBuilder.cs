using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;

using Autodesk.Windows;
using Intellidesk.AcadNet.Data.Models;
using Intellidesk.Infrastructure;
using Intellidesk.Infrastructure.Interfaces;

namespace Intellidesk.AcadNet.Services
{
    public static class RibbonTabExtensions
    {
        //private static ICommand _rbnOpenLayoutExplorer;
        //public static Lazy<ICommand> RbnOpenLayoutExplorer
        //{
        //    get
        //    {
        //        var buildService = PluginBootstrapper.PluginContainer.Resolve<IUIBuildService>();
        //        var result = _rbnOpenLayoutExplorer ??
        //                     (_rbnOpenLayoutExplorer = buildService.MainViewModel.RbnOpenLayoutExplorer);
        //        return buildService.MainViewModel.RbnOpenLayoutExplorer;
        //    }
        //}

        /// <summary> Creare RibbonTab </summary>
        public static RibbonTab Load(this RibbonControl ribbonControl, string ribbonTabName, MainViewModel mainViewModel = null, InteractionRequestViewModel interactionRequestViewModel = null)
        //Action<object, DocumentCollectionEventArgs> onTabActivated = null)
        {
            // Reference to project resources
            var resourceManager = new ResourceManager("IntelliDesk.AcadNet.Properties.Resources", typeof(Resources).Assembly);

            //var NetToolsResourceManager = UIManager.GetResourceManager();
            //ribbonControl.DataContext = MainViewModel;

            var ribbonTab = new RibbonTab { Title = ribbonTabName, Id = "ID_" + ribbonTabName };
            ribbonControl.Tabs.Add(ribbonTab);

            #region "Layouts Catalog"

            // create panel source and create panel
            var panelSource1 = new RibbonPanelSource { Title = "Layouts Catalog" };
            var btnLayoutExplorer = new RibbonButton
            {
                Text = "Layout\nExplorer",
                Name = "LayoutExplorer",
                Id = "RbLayoutExplorerId",
                CommandHandler = mainViewModel != null ? mainViewModel.RbnOpenLayoutExplorer : null,
                ShowText = true,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("explorer_32")),
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
            ////btnLayoutExplorer.CommandHandler = RbnOpenLayoutExplorer;
            //btnLayoutExplorer.CommandHandlerBinding = new Binding("RbnOpenLayoutExplorer") 
            //{ Source = new Lazy<MainViewModel>(() => buildService.MainViewModel) };

            panelSource1.Items.Add(btnLayoutExplorer);
            var panel1 = new RibbonPanel { Source = panelSource1 };
            ribbonTab.Panels.Add(panel1);

            // Create a Command Item that the Dialog Launcher can use, for this test it is just a place holder.
            var bitmapImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("prop_arrow"));
            var dialogLauncherButton = new RibbonButton { Name = "Options", Image = bitmapImage, LargeImage = bitmapImage };
            panelSource1.DialogLauncher = dialogLauncherButton;

            var rowPanel2 = new RibbonRowPanel() { Name = "Panel2", Id = "Panel2" };
            // Add new panel into our tab
            var rlLayout = new RibbonLabel() { Text = "Active Layout:", Width = 95 };
            var rlConfig = new RibbonLabel() { Text = "Active Settings:", Width = 95 };

            #region "LoadLayouts"

            var layoutComboBox = new RibbonCombo
            {
                Name = "Layout Gallery",
                Id = "LayoutRibbonComboId",
                Width = 200,
                ToolTip = new RibbonToolTip
                {
                    Title = "Active Layout",
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
                    Content = "The configuration set currently being used for FSA and layout catalog options"
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
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("lsdsRunFsa_32")),
                //Image = LoadImage("upload_32"), //Images.GetBitmap((Bitmap)_resourceManager.GetObject("upload_32")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Apply",
                ToolTip = new RibbonToolTip
                {
                    Content = "Apply the Frames Simplification Algorithm to the active layout",
                    IsHelpEnabled = false,
                    Title = "Apply"
                }
            };
            panelSource1.Items.Add(rbApply);

            var rbUpload = new RibbonButton
            {
                CommandHandler = mainViewModel != null ? mainViewModel.RbnUpload : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("lsdsUpload_32")),
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Upload",
                ToolTip = new RibbonToolTip
                {
                    Content = "Upload the Frames Simplification Algorithm (FSA) to the DB",
                    IsHelpEnabled = false,
                    Title = "Upload"
                }
            };
            panelSource1.Items.Add(rbUpload);

            var rbExport = new RibbonButton
            {
                CommandHandler = mainViewModel != null ? mainViewModel.RbnExport : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("lsdsExport_32")), //Image = LoadImage("icon_32")
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                ShowText = true,
                Text = "Export",
                ToolTip = new RibbonToolTip
                {
                    Content = "Save the active layout to the layouts catalog",
                    IsHelpEnabled = false,
                    Title = "Export"
                }
            };

            #endregion

            panelSource1.Items.Add(rbExport);

            #endregion

            #region "Commands"

            var panelSource2 = new RibbonPanelSource { Title = "Commands" };

            panelSource2.Items.Add(new RibbonButton
            {
                CommandHandler = mainViewModel != null ? mainViewModel.RbnMapInfoGetData : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("mapGetDwg_32")),
                Name = "LoadAsmade",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Load Asmade",
                ToolTip = new RibbonToolTip
                {
                    Content = "Executes an action or command in LoadAsmade (layout quality assurance, " +
                              "tools per side of the bay, layout/schedule conflicts, layout per date, etc.)",
                    IsHelpEnabled = false,
                    Title = "Load Asmade"
                }
            });

            panelSource2.Items.Add(new RibbonButton
            {
                CommandHandler = mainViewModel != null ? mainViewModel.RbnSentToMap : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("mapView_32")),
                Name = "ViewMap",
                Orientation = Orientation.Vertical,
                ShowText = true,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                Text = "View Map",
                ToolTip = new RibbonToolTip
                {
                    Content = "Display the results in a tabular report",
                    IsHelpEnabled = false,
                    Title = "View Report"
                }
            });

            panelSource2.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("attach_32")),
                Name = "ApplyToDrawing",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Apply to drawing",
                ToolTip = new RibbonToolTip
                {
                    Content = "Apply the analysis results as graphical annotations on the active drawing",
                    IsHelpEnabled = false,
                    Title = "Apply to drawing"
                }
            });

            var rbLoadResults = new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("results_load_32")),
                Name = "LoadResults",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Load Results",
                ToolTip = new RibbonToolTip
                {
                    Content = "Re-use an analysis results instead of executing it again",
                    IsHelpEnabled = false,
                    Title = "Load Results"
                }
            };
            panelSource2.Items.Add(rbLoadResults);

            var rbSaveResults = new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("results_save_32")),
                Name = "SaveResults",
                Orientation = Orientation.Vertical,
                Size = RibbonItemSize.Large,
                ShowText = true,
                ShowImage = true,
                Text = "Save Results",
                ToolTip = new RibbonToolTip
                {
                    Content = "Save an analysis results instead of executing it again",
                    IsHelpEnabled = false,
                    Title = "Save Results"
                }
            };
            panelSource2.Items.Add(rbSaveResults);

            var panel2 = new RibbonPanel { Source = panelSource2 };
            ribbonTab.Panels.Add(panel2);

            #endregion

            #region "Settings,Help,Notify"

            var panelSource3 = new RibbonPanelSource { Title = "Settings" };

            panelSource3.Items.Add(new RibbonButton
            {
                Id = "RefreshId",
                CommandHandler = mainViewModel != null ? mainViewModel.RbnRefresh : null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("refresh"),2,1),
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

            panelSource3.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("options_32")),
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
            
            panelSource3.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("help_32")),
                Name = "Help",
                Orientation = Orientation.Vertical,
                ShowText = true,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                Text = "Help",
                ToolTip = new RibbonToolTip
                {
                    Content = "Usage manuals and ‘About’",
                    IsHelpEnabled = false,
                    Title = "Help"
                }
            });

            var panel3 = new RibbonPanel { Source = panelSource3 };
            ribbonTab.Panels.Add(panel3);

            var panelSource4 = new RibbonPanelSource { Title = "Notification" };

            var aggregator = PluginBootstrapper.PluginContainer.Resolve<IEventAggregator>();
            aggregator.GetEvent<NotifyMessageStringEvent>().Subscribe(TabNotifyUpdateMessage);

            panelSource4.Items.Add(new RibbonButton
            {
                CommandHandler = null,
                CommandParameter = null,
                LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("information_32")),
                Name = "Notifications",
                Orientation = Orientation.Vertical,
                ShowText = true,
                Size = RibbonItemSize.Large,
                ShowImage = true,
                Text = "Init...",
                ToolTip = new RibbonToolTip
                {
                    Content = "no tasks",
                    IsHelpEnabled = false,
                    Title = "Notifications"
                }
            });

            //panelSource4.Items.Add(new RibbonItem() {});

            var panel4 = new RibbonPanel { Source = panelSource4 };
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
            var resourceManager = new ResourceManager("IntelliDesk.AcadNet.Properties.Resources", typeof(Resources).Assembly);
            var ribbonTab = ribbonControl.Tabs.First(x => x.Id == "ID_Partner");
            var btn = ribbonTab.Panels[3].Source.Items[0];
            btn.Text = notifyMessage ?? "Ready";
            btn.LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject(
                notifyMessage != null ? "information_32" : "accept_32"));
        }

        public static void TabNotifyUpdateMessage(string notifyMessage = null)
        {
            var args = PluginBootstrapper.PluginContainer.Resolve<ITaskArguments>();

            var ribbonControl = ComponentManager.Ribbon;
            var resourceManager = new ResourceManager("IntelliDesk.AcadNet.Properties.Resources", typeof(Resources).Assembly);
            var ribbonTab = ribbonControl.Tabs.First(x => x.Id == "ID_Partner");
            var btn = ribbonTab.Panels[3].Source.Items[0];
            btn.Text = notifyMessage ?? "Ready";
            if (args.ErrorInfo.Any())
                btn.LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject("exclamation_32"));
            else
            {
                args = null;
                btn.LargeImage = ImageHelper.GetBitmap((Bitmap)resourceManager.GetObject(
                    notifyMessage != null ? "information_32" : "accept_32"));
            }
        }

        public static RibbonTab LoadData(this RibbonControl ribbonControl, MainViewModel mainViewModel)
        {
            //ribbonControl.DataContext = interactionRequestViewModel;
            var ribbonTab = ribbonControl.Tabs.First(x => x.Id == "ID_Partner");

            var btnRefresh = (RibbonButton)ribbonTab.Panels[2].FindItem("RefreshId");
            btnRefresh.CommandHandler = mainViewModel.RbnRefresh;

            var btnLayoutExplorer = (RibbonButton)ribbonTab.Panels[0].FindItem("RbLayoutExplorerId");
            //btnOpenLayoutExplorer.CommandHandlerBinding = new Binding("RbnSampleCommand") { Source = mainViewModel };
            btnLayoutExplorer.CommandHandlerBinding =
                new Binding("RbnOpenLayoutExplorer") { Source = mainViewModel };
            //btnLayoutExplorer.CommandHandler = mainViewModel.RbnOpenLayoutExplorer;

            var layoutComboBox = (RibbonCombo)ribbonTab.Panels[0].FindItem("LayoutRibbonComboId");
            layoutComboBox.DropDownOpened += OnlayoutComboBoxDropDownOpened;
            layoutComboBox.DropDownClosed += OnlayoutComboBoxDropDownClosed;
            layoutComboBox.CurrentChanged += mainViewModel.OnlayoutComboBoxCurrentChanged;

            Binding bind;
            if (mainViewModel.RibbonLayoutItems != null && mainViewModel.RibbonLayoutItems.Count > 0)
            {
                bind = new Binding("RibbonLayoutItems") { Source = mainViewModel };
                layoutComboBox.ItemsBinding = bind;
                layoutComboBox.Current = mainViewModel.RibbonLayoutItems.FirstOrDefault(x => x.Id == "none");
            }

            bind = new Binding("CurrentRibbonLayout") { Source = mainViewModel }; //ComponentManager.Ribbon.DataContext
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
    }
}
