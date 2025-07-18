using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;

using Intellidesk.AcadNet.Tools;
using Intellidesk.AcadNet.Tools.Properties;
using Intellidesk.NetTools;

using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Forms.Control;
using Exception = System.Exception;
using FlowDirection = System.Windows.FlowDirection;
using Orientation = System.Windows.Controls.Orientation;
using RibbonButton = Autodesk.Windows.RibbonButton;
using RibbonControl = Autodesk.Windows.RibbonControl;
using RibbonPanelSource = Autodesk.AutoCAD.Customization.RibbonPanelSource;
using Size = System.Drawing.Size;

[assembly: CommandClass(typeof(UIManager))]
namespace Intellidesk.AcadNet.Tools
{
    #region "Delegates & EventArgs"

    public delegate void UIControlClickEventHandler(object sender, UIControlEventArgs e);

    public delegate void PaletteEventHandler(ITabPalette sender);

    public delegate void PaletteSetStateEventHandler(PaletteSet sender, PaletteSetStateEventArgs args);

    public class PaletteSetStateEventArgs : EventArgs
    {
        public PaletteSetStateEventArgs(StateEventIndex newState)
        {
            NewState = newState;
        }
        public StateEventIndex NewState { get; private set; }
    }

    public class UIControlEventArgs : RoutedEventArgs
    {
        public UIControlEventArgs(object sender, string commandArgs)
        {
            CurrentObject = sender;
            Command = commandArgs;
        }
        public object CurrentObject { get; private set; }
        public string Command { get; private set; }
    }

    //public delegate void UIControlClickEventHandler(object sender, UIControlEventArgs e);

    #endregion

    #region "Extensions"

    /// <summary> Extensions for RibbonControl </summary>
    public static partial class Extensions
    {
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
    }

    #endregion

    #region "Interfaces"

    public interface ITabPalette
    {
        event UIControlClickEventHandler OpenClickEvent;
        event UIControlClickEventHandler SaveClickEvent;
        event UIControlClickEventHandler DeleteClickEvent;
        event UIControlClickEventHandler CopyClickEvent;
        event UIControlClickEventHandler ParseClickEvent;
        event UIControlClickEventHandler PurgeClickEvent;
        event UIControlClickEventHandler SaveChangedEvent;

        bool Current { get; set; }
        string Name { get; set; }
        string Header { get; set; }
        object ParentControl { get; set; }
        object PalleteControl { get; set; }
        int UniId { get; set; }
        int TabId { get; set; }
        bool Visible { get; set; }
        bool Complete { get; set; }
        [Browsable(true)]
        string Comment { get; set; }
        int Width { get; set; }

        void OnActivate(object tObj = null);
        void OnDeactivate();
        void Refresh(bool flagManualChange = false);
        void Apply();

    }

    public interface ITabControl
    {
        byte TabId { get; set; }
        string Name { get; set; }
    }

    public enum PaletteViewStatus
    {
        Docked, Free, Init, Loaded, ToBeClose
    }

    public interface IPaletteView
    {
        //PaletteViewStatus Status { get; set; }
        PaletteSet PaletteSetParent { get; set; }
    }

    public interface IPaletteSet
    {
        event EventHandler PaletteSetClosed;
        //UserControl CurrentView { get; set; }
        PaletteState State { get; set; }
        DockSides DockSides { get; set; }
        bool FullView { get; set; }
    }

    public interface IAcadNetTools
    {
        void OnParseObjectsClick(object sender, UIControlEventArgs args);
        void OnToolsParseObjects(ObjectId sender, ActionArguments args);
        void OnToolsParseBlock(ObjectId sender, ActionArguments args);
    }

    //public interface ITabControl
    //{
    //    byte TabId { get; set; }
    //    string Name { get; set; }
    //    //Sub MakeDownTabId()
    //    //event MenuRotateCnt_EventEventHandler MenuRotateCnt_Event;
    //    //delegate void MenuRotateCnt_EventEventHandler(GetSelectOptions tOption);
    //}

    public interface ITaskScreen
    {
        void AddTask(string taskName, Func<bool> act, string onSuccessMessage);
        void AddMessage(string message);
        void AddMessage(string message, Action action); //Expression<Func<object, bool>> exp
        void Complete();
        void StartAndWait();
    }

    #endregion

    /// <summary> UIManager </summary>
    public class UIManager
    {
        public static ITaskScreen TaskScreen;
        private static ManualResetEvent ResetTaskScreenCreated;
        private static Thread TaskScreenThread;

        //public static event UIControlClickEventHandler ButtonsClickEvent;

        //public static void OnLayoutButtonsClick(Object sender, UIControlEventArgs e)
        //{
        //    var handler = ButtonsClickEvent;
        //    if (handler != null) handler(sender, e);
        //}

        #region "Build Ribbon control"

        /// <summary> Current ribbonTab name </summary>
        public static string CurrentRibbonTabName;
        public static ResourceManager ResourceManager;


        public static ResourceManager GetResourceManager()
        {
            return new ResourceManager("AcadNet.Tools.Properties.Resources", typeof(Resources).Assembly);
        }

        /// <summary> Creare RibbonTab </summary>
        public static RibbonTab CreateRibbonTab(string ribbonTabName, object viewModel)
        {
            RibbonTab ribbonTab = null;
            ResourceManager = new ResourceManager("AcadNet.Tools.Properties.Resources", typeof(Resources).Assembly);
            CurrentRibbonTabName = ribbonTabName;

            //ComponentManager.Ribbon contains all others controls
            //RibbonControl = ComponentManager.Ribbon;

            //RibbonControl.DataContext = viewModel;
            ribbonTab = new RibbonTab { Title = CurrentRibbonTabName, Id = "ID_" + CurrentRibbonTabName };
            ComponentManager.Ribbon.Tabs.Add(ribbonTab);
            return ribbonTab;
        }

        private static Autodesk.Windows.RibbonPanelSource XAddButonLauncher(Autodesk.Windows.RibbonPanelSource rps)
        {
            var dialogLauncherButton = new RibbonButton { Name = "TestCommand" };
            rps.DialogLauncher = dialogLauncherButton;
            return rps;
        }

        static void RemoveRibbonTab(string ribbonTabName)
        {
            try
            {
                var ribCntrl = ComponentManager.Ribbon;
                // to do iteration by tabs
                foreach (var tab in ribCntrl.Tabs)
                {
                    if (tab.Id.Equals(ribbonTabName + "_ID") & tab.Title.Equals(ribbonTabName))
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
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
            }
        }

        #endregion

        #region "PaletteSets"

        /// <summary> Our static event to be fired on evevts as such as close, loaded, show </summary>
        public static event PaletteSetStateEventHandler PaletteSetChanged;

        /// <summary> Hold a static pointers to the palette/view set being closed </summary>
        public static PaletteSetControl PaletteSetCurrent = null;
        public static object PaletteFormCurrent;
        public static object PaletteViewCurrent;

        /// <summary> IsPaletteSetLoaded not enable create two palettes </summary>
        public static bool IsPaletteSetLoaded = false;

        /// <summary> Palette Set class </summary>
        public class PaletteSetControl : PaletteSet
        {
            /// <summary> Minimize size of palette </summary>
            public Size SizeMin { get; set; }
            /// <summary> Maximize size of palette </summary>
            public Size SizeMax { get; set; }

            /// <summary> State of palette </summary>
            public StateEventIndex PaletteState { get; set; }

            /// <summary> ctor </summary>
            public PaletteSetControl(string name, Guid toolID)
                : base(name, toolID)
            {
                StateChanged += OnStateChanged;
            }

            /// <summary> occur on event the State changed of palette </summary>
            public void OnStateChanged(object sender, Autodesk.AutoCAD.Windows.PaletteSetStateEventArgs e)
            {
                switch (e.NewState)
                {
                    // On hide we fire a command to check the state properly
                    case StateEventIndex.Hide:
                        CommandManager.SendToExecute("CHECKPALETTESETCLOSE ");
                        break;
                    case StateEventIndex.Show:
                        if (((PaletteSetControl)sender).Visible)
                            CommandManager.SendToExecute("CHECKPALETTESETLOADED ");
                        break;
                }
                // Set the static property to point to our palette
                PaletteSetCurrent = sender as PaletteSetControl;
                PaletteSetCurrent.PaletteState = e.NewState;
            }
        }

        /// <summary> Create AutoCAD Paletteset </summary>
        public static void CreatePaletteSet<T>(string paletteName, object viewModel, Action<object> addCustomEvents = null) where T : class
        {
            if (PaletteSetCurrent != null) return;

            PaletteSetCurrent = new PaletteSetControl(paletteName, new Guid("06B75904-219F-4CA5-AC02-E3970A645F18"))
                               {
                                   Style = PaletteSetStyles.NameEditable | PaletteSetStyles.ShowPropertiesMenu |
                                           PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton,
                                   MinimumSize = new Size(300, 300),
                                   Dock = (DockSides)((int)DockSides.Left + (int)DockSides.Right)
                               };

            if (typeof(T).BaseType == typeof(Control))
            {
                //Set the static property to point to our palette
                PaletteFormCurrent = Activator.CreateInstance(typeof(T)) as Control;

                // add Control to PaletteSet
                PaletteSetCurrent.Add(ProjectManager.Name + ProjectManager.Version, (Control)PaletteFormCurrent);

                // add custom events from external function
                if (addCustomEvents != null) addCustomEvents(PaletteFormCurrent);
            }

            if (typeof(T).BaseType == typeof(UserControl))
            {
                PaletteViewCurrent = Activator.CreateInstance(typeof(T), new[] { viewModel }) as UserControl; //ComponentManager.Ribbon.DataContext

                // add Control to PaletteSet
                if (PaletteViewCurrent != null)
                    PaletteSetCurrent.AddVisual(paletteName, (UserControl)PaletteViewCurrent);

                // add custom events from external function
                if (addCustomEvents != null)
                    addCustomEvents(PaletteViewCurrent);
            }
        }

        /// <summary> Create AutoCAD Paletteset </summary>
        public static void CreatePaletteSets(UserControl us, string paletteName, Action<object> addCustomEvents = null)
        {
            if (PaletteSetCurrent != null) return;

            PaletteSetCurrent = new PaletteSetControl(paletteName, new Guid("06B75904-219F-4CA5-AC02-E3970A645F18"))
            {
                Style = PaletteSetStyles.NameEditable | PaletteSetStyles.ShowPropertiesMenu |
                        PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton,
                MinimumSize = new Size(300, 300),
                Dock = (DockSides)((int)DockSides.Left + (int)DockSides.Right)
            };

            PaletteViewCurrent = us;

            // add Control to PaletteSet
            if (PaletteViewCurrent != null)
                PaletteSetCurrent.AddVisual(paletteName, (UserControl)PaletteViewCurrent);

            // add custom events from external function
            if (addCustomEvents != null)
                addCustomEvents(PaletteViewCurrent);
        }

        /// <summary> Check palette on state the "Close" </summary>
        [CommandMethod("CHECKPALETTESETCLOSE", CommandFlags.NoHistory)]
        public static void CheckPaletteSetStateClose()
        {
            // Get the static instance set
            var ps = PaletteSetCurrent;

            // If it's invisible, it has been closed
            if (ps != null && !ps.Visible)
            {
                // Set the static instance to null and fire the subscribed event
                if (ps.Size != new Size(0, 0))
                    PaletteSetCurrent = null;
                if (PaletteSetChanged != null)
                    PaletteSetChanged(ps, new PaletteSetStateEventArgs(StateEventIndex.Hide));
            }
        }

        /// <summary> Check palette on state the "Loaded" </summary>
        [CommandMethod("CHECKPALETTESETLOADED", CommandFlags.NoHistory)]
        public static void CheckPaletteSetStateLoaded()
        {
            // Get the static instance set
            var ps = PaletteSetCurrent;

            // If it's invisible, it has been closed
            if (ps != null && ps.Visible)
            {
                // Set the static instance to null and fire the subscribed event
                //PaletteSetCurrent = null;

                if (PaletteSetChanged != null)
                    PaletteSetChanged(ps, new PaletteSetStateEventArgs(StateEventIndex.Show));
            }
        }

        #endregion

        #region "Ribbon elements"

        public static bool RibbonAdded = false;

        public static CustomizationSection GetCustomizationSection()
        {
            CustomizationSection cs = null;
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                cs = new CustomizationSection((string)Application.GetSystemVariable("MENUNAME"));
                var curWorkspace = (string)Application.GetSystemVariable("WSCURRENT");
            }
            catch (Exception ex)
            {
                ed.WriteMessage(Environment.NewLine + ex.Message);
            }
            return cs;
        }

        //AddRibbonPanel(cs, "ANAW", "AcadNetAddinWizard1");
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
                    panelSrc.Id = panelSrc.ElementID = panelName + "ID_" + panelName;
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
                    rt = new RibbonTab { Title = tabName, Id = "ID_" + tabName.ToUpper() + ProjectManager.Name };
                    rc.Tabs.Add(rt);
                    //rt.IsActive = true;
                }
                catch (Exception ex)
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
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
                Orientation = Orientation.Vertical,
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
            var tb = new WpfTextBox(150, 15, 17, 5)
            {
                IsEmptyTextValid = false,
                AcceptTextOnLostFocus = true,
                InvokesCommand = true,
                CommandHandler = new TextboxCommandHandler()
            };
            rps.Items.Add(tb);

            // Set our tab to be active
            rt.IsActive = true;
        }

        #endregion

        #region "Methods"

        /// <summary> Get image from resources </summary>
        private static BitmapImage LoadImage(string imageName)
        {
            return new BitmapImage(new Uri("pack://application:,,,/X;component/Resources/" + imageName + ".png"));
        }

        /// <summary> Deciding of problem: Autocad main window not get focus after palette activity </summary>
        public static void AcadWindowSetFocus()
        {
            SetFocus(Application.MainWindow.Handle);
        }
        /// <summary> WriteMessage </summary>
        public static void WriteMessage(string s)
        {
            // A simple helper to write to the command-line
            var doc = Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage(s);
        }
        public static void Alert(string alert)
        {
            Application.ShowAlertDialog(alert);
        }
        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        /// <summary> DisplayTaskScreen </summary>
        public static void DisplayTaskScreen(string command = "", object arguments = null)
        {
            DisplayTaskScreen(new CommandArguments { Command = command, Arguments = arguments });
        }

        /// <summary> DisplayTaskScreen </summary>
        public static void DisplayTaskScreen(CommandArguments commandArguments)
        {
            ResetTaskScreenCreated = new ManualResetEvent(false);

            // Create a new thread for the splash screen to run on
            TaskScreenThread = new Thread(() => ShowScreen(commandArguments));
            TaskScreenThread.SetApartmentState(ApartmentState.STA);
            TaskScreenThread.IsBackground = true;
            TaskScreenThread.Name = "Splash Screen";
            TaskScreenThread.Start();
        }

        private static void ShowScreen(CommandArguments commandArguments)
        {
            // Create the window
            var taskWindow = new TaskScreen();
            TaskScreen = taskWindow;

            // Show it
            Application.ShowModelessWindow(Application.MainWindow.Handle, taskWindow, false);
            //animatedSplashScreenWindow.Show();

            // Now that the window is created, allow the rest of the startup to run
            ResetTaskScreenCreated.Set();
            System.Windows.Threading.Dispatcher.Run();

            if (commandArguments.Command != "")
                CommandManager.SendToExecute(commandArguments);//new CommandArguments { Command = "PARTNERSTART", Arguments = this }
        }

        #endregion
    }

    #region "Commands & handlers"

    class ComboBoxCommandHandler : ICommand
    {
#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        public bool CanExecute(object parameter)
        {
            // Yes, we can execute
            return true;
        }

        public void Execute(object parameter)
        {
            // Dump the WpfComboBox contents to the command-line
            var tb = parameter as WpfComboBox;
            if (tb != null)
            {
                UIManager.WriteMessage("\nRibbon ComboBox: " + tb.Name + "\n");
            }
        }
    }

    class TextboxCommandHandler : ICommand
    {
#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        public bool CanExecute(object parameter)
        {
            // Yes, we can execute
            return true;
        }

        public void Execute(object parameter)
        {
            // Dump the textbox contents to the command-line
            var tb = parameter as WpfTextBox;
            if (tb != null)
            {
                UIManager.WriteMessage("\nRibbon Textbox: " + tb.GetTextWithoutNewlines() + "\n");
                tb.ClearText();
            }
        }
    }

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        private PaletteSet _ps;
        static ResourceManager _resourceManager;

        [CommandMethod("Test")]
        public void ShowWpfPalette()
        {
            _resourceManager = new ResourceManager("WPFPalette.MyResource", this.GetType().Assembly);
            if (Application.Version.Major == 17)
            {
                if (Application.Version.Minor == 1)
                    return; //AutoCAD 2008
            }

            if (_ps == null)
            {
                _ps = new PaletteSet("WPF Palette")
                {
                    Size = new Size(400, 600),
                    DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right)
                };
                var uc = new MyWPFUserControl();

                //Autodesk.Windows.RibbonControl ribControl = Autodesk.Windows.ComponentManager.Ribbon;

                var ribControl = new RibbonControl();

                var ribTab = new RibbonTab { Title = "Test", Id = "Test" };
                ribControl.Tabs.Add(ribTab);

                var ribSourcePanel = new Autodesk.Windows.RibbonPanelSource
                {
                    Title = "My Tools",
                    DialogLauncher = new RibbonCommandItem { CommandHandler = new AdskCommandHandler() }
                };

                //Add a Panel
                var ribPanel = new RibbonPanel { Source = ribSourcePanel };
                ribTab.Panels.Add(ribPanel);

                //Create button
                var ribButton1 = new RibbonButton
                {
                    Text = "Line" + "\n" + "Generator",
                    CommandParameter = "Line ",
                    ShowText = true,
                    LargeImage = ImageHelper.GetBitmap((Bitmap)_resourceManager.GetObject("X_16x16.bmp")),
                    Image = ImageHelper.GetBitmap((Bitmap)_resourceManager.GetObject("X_16x16.bmp")),
                    Size = RibbonItemSize.Large,
                    Orientation = System.Windows.Controls.Orientation.Vertical,
                    ShowImage = true
                };
                ribButton1.ShowText = true;
                ribButton1.CommandHandler = new AdskCommandHandler();
                ribSourcePanel.Items.Add(ribButton1);

                uc.Content = ribControl;

                _ps.AddVisual("Test", uc);
            }

            _ps.KeepFocus = true;
            _ps.Visible = true;
        }
    }

    public class AdskCommandHandler : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }
        event EventHandler ICommand.CanExecuteChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }
        public void Execute(object parameter)
        {
            var ribBtn = parameter as RibbonButton;
            if (ribBtn != null)
            {
                string sCmd = string.Empty;
                string sSubCmd = ribBtn.CommandParameter.ToString();
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute(sSubCmd, true, false, true);
            }
        }
    }

    #endregion

    #region "UI Controls"

    class WpfComboBox : RibbonCombo
    {
        public WpfComboBox() { }

        protected override void OnEditableTextChanged(RibbonPropertyChangedEventArgs args)
        {
            base.OnEditableTextChanged(args);
            UIManager.WriteMessage("\nRibbon ComboBox new value: " + args.NewValue + "\n");
        }
    }

    public class MyStateControl : ButtonBase
    {
        public MyStateControl() : base() { }
        public Boolean State
        {
            get { return (Boolean)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
          "State", typeof(Boolean), typeof(MyStateControl), new PropertyMetadata(false));
    }

    public class WpfButton : RibbonButton
    {
        public WpfButton()
        {
            EventManager.RegisterClassHandler(typeof(ButtonBase), UIElement.GotFocusEvent,
                new RoutedEventHandler(OnGotFocus));
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            UIManager.WriteMessage("IsEnable");
        }

        public static readonly DependencyProperty IsBubbleSourceProperty = DependencyProperty.RegisterAttached("IsBubbleSource",
            typeof(Boolean), typeof(WpfButton), new PropertyMetadata(false));

        public static void SetIsBubbleSource(UIElement element, Boolean value)
        {
            element.SetValue(IsBubbleSourceProperty, value);
        }

        public static Boolean GetIsBubbleSource(UIElement element)
        {
            return (Boolean)element.GetValue(IsBubbleSourceProperty);
        }



    }

    public class WpfTextBox : RibbonTextBox
    {
        double _baseHeight;
        double _baseWidth;
        double _heightPadding;
        double _widthPadding;
        bool _textChanging = false;

        public WpfTextBox(double width, double height, double widthPadding, double heightPadding)
        {
            // Set some member variables, some of which
            // we also use to set the TextBox dimensions

            _baseWidth = width;
            _baseHeight = height;
            _widthPadding = widthPadding;
            _heightPadding = heightPadding;

            Width = width;
            Height = height;
            MinWidth = width;

            // Register our focus-related event handlers
            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent,
                new RoutedEventHandler(OnGotFocus));

            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.LostKeyboardFocusEvent,
              new RoutedEventHandler(OnLostFocus));

            // And out additional TextChanged event handler
            EventManager.RegisterClassHandler(typeof(TextBox), TextBoxBase.TextChangedEvent,
                new RoutedEventHandler(OnTextChanged));
        }

        public string GetTextWithoutNewlines()
        {
            // Return the contained text without newline characters
            return TextValue.ReplaceNewlinesWithSpaces();
        }

        public void ClearText()
        {
            TextValue = "";
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            if (!_textChanging && e != null && e.Source != null)
            {
                var tb = e.Source as TextBox;
                if (tb != null)
                {
                    // We need the typeface to calculate the text width
                    var faces = tb.FontFamily.GetTypefaces();
                    Typeface face = null;
                    foreach (Typeface tf in faces)
                    {
                        if (tf != null)
                        {
                            face = tf;
                            break;
                        }
                    }

                    // Get the last line of text, to see how long it is
                    var text = tb.Text.GetLastLine();

                    // Calculate the width of this last line of text
                    if (face != null)
                    {
                        var ft = new FormattedText(
                            text,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            face,
                            tb.FontSize * (96.0 / 72.0),
                            Brushes.Black
                            );

                        // If the width of the last line of text
                        // is over our boundary (the width of the box
                        // minus some padding), then start the next
                        // line

                        if (ft.Width - _widthPadding > _baseWidth)
                        {
                            // Set our flag to stop re-entry of the event
                            // handler, then replace the last space in the
                            // TextBox contents with a newline

                            _textChanging = true;
                            tb.Text =
                                tb.Text.InsertNewlineAtLastSpace();
                            _textChanging = false;

                            // Set the cursor to be at the end of our text
                            // so that typing continues properly

                            tb.SelectionStart = tb.Text.Length;
                        }
                    }

                    // Find the number of lines of text
                    var lines = tb.Text.GetLineCount();

                    // Change the height based on the number of lines
                    tb.Height = _heightPadding + (lines * _baseHeight);
                    tb.MinLines = lines;
                }
            }
        }

        // Both events call the same helper, with a custom message
        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChange(sender, e, "\nTextbox got focus :)\n");
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChange(sender, e, "\nTextbox lost focus :(\n");
        }

        // Our helper function to print  a message only when
        // our custom textbox exists
        private void OnFocusChange(object sender, RoutedEventArgs e, string msg)
        {
            if (e != null && e.Source != null)
            {
                var tb = e.Source as TextBox;
                if (tb != null)
                {
                    var mtb = tb.DataContext as WpfTextBox;
                    if (mtb != null)
                    {
                        UIManager.WriteMessage(msg);
                    }
                }
            }
        }
    }

    #endregion

    #region "Extensions"

    public static class BindingExtensions
    {
        public static readonly DependencyProperty MyIsEnabledProperty =
            DependencyProperty.RegisterAttached("MyIsEnabled", typeof(RibbonButton), typeof(BindingExtensions));

        public static bool GetMyIsEnabled(this UIElement element)
        {
            return (bool)element.GetValue(MyIsEnabledProperty);
        }

        public static void SetMyIsEnabled(this UIElement element, bool title)
        {
            element.SetValue(MyIsEnabledProperty, title);
        }

        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.RegisterAttached("Format", typeof(string), typeof(BindingExtensions));

        public static string GetFormat(this Binding binding)
        {
            if (binding.Converter is StringFormatConverter)
                return ((StringFormatConverter)binding.Converter).Format;
            return string.Empty;
        }

        public static void SetFormat(this Binding binding, string stringFormat)
        {
            if (binding.Converter == null)
                binding.Converter = new StringFormatConverter(stringFormat);
        }

        private class StringFormatConverter : IValueConverter
        {
            public string Format { get; private set; }

            public StringFormatConverter(string format)
            {
                Format = format;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return string.Format(culture, GetEffectiveStringFormat(Format), value);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }

            private static string GetEffectiveStringFormat(string stringFormat)
            {
                if (stringFormat.IndexOf('{') < 0)
                {
                    stringFormat = "{0:" + stringFormat + "}";
                }
                return stringFormat;
            }
        }
    }

    // String-related extension methods
    static class StringExtensions
    {
        const string Newline = "\r\n";

        public static string InsertNewlineAtLastSpace(this string s)
        {
            // If the string contains a space, replace it
            // with a newline character, otherwise we simply
            // append the newline to the string

            string ret;
            if (s.Contains(" "))
            {
                int index = s.LastIndexOf(" ");
                ret =
                  s.Substring(0, index) + Newline +
                  s.Substring(index + 1);
            }
            else
            {
                ret = s + Newline;
            }
            return ret;
        }

        public static string ReplaceNewlinesWithSpaces(this string s)
        {
            // Replace all the newlines with spaces
            if (String.IsNullOrEmpty(s))
                return s;
            return s.Replace(Newline, " ");
        }

        public static string GetLastLine(this string s)
        {
            // Return the last line of the text (or
            // the whole thing if there's no newline in it)
            var ret = s.Contains(Newline) ? s.Substring(s.LastIndexOf(Newline) + Newline.Length) : s;
            return ret;
        }

        public static int GetLineCount(this string s)
        {
            // Count the number of lines by checking the
            // overall length of the string against the
            // string without newline sequences in it
            return ((s.Length - s.Replace(Newline, "").Length) / Newline.Length) + 1;
        }
    }

    #endregion
}



