using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using Intellidesk.AcadNet.Services.Commands;
using System.Drawing;
using System.Resources;
using System.Windows.Controls;

namespace Intellidesk.AcadNet.Core
{
    public class MyCommands
    {
        private PaletteSet _ps;
        ResourceManager _resourceManager;

        [CommandMethod("Test")]
        public void ShowWpfPalette()
        {
            _resourceManager = new ResourceManager("Intellidesk.AcadNet.Properties.Resources", this.GetType().Assembly);
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
                //var uc = new MyWPFUserControl();

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
                    Text = "Line\nGenerator",
                    CommandParameter = "Line ",
                    ShowText = true,
                    //LargeImage = ImageHelper.GetBitmap((Bitmap)_resourceManager.GetObject("shield_16")),
                    //Image = ImageHelper.GetBitmap((Bitmap)_resourceManager.GetObject("shield_16")),
                    Size = RibbonItemSize.Large,
                    Orientation = Orientation.Vertical,
                    ShowImage = true
                };
                ribButton1.ShowText = true;
                ribButton1.CommandHandler = new AdskCommandHandler();
                ribSourcePanel.Items.Add(ribButton1);

                //uc.Content = ribControl;

                //_ps.AddVisual("Test", uc);
            }

            _ps.KeepFocus = true;
            _ps.Visible = true;
        }
    }
}