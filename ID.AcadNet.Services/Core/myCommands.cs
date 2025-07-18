// (C) Copyright 2011 by Microsoft 
//
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using System.IO;
using System.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Resources;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Drawing;
using Autodesk.Windows;
using Autodesk.AutoCAD.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

// This line is not mandatory, but improves loading performances
//[assembly: CommandClass(typeof(WPFPalette.MyCommands))]

namespace WPFPalette
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        private Autodesk.AutoCAD.Windows.PaletteSet _ps;
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
                        DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right)
                    };
                var uc = new MyWPFUserControl();

                //Autodesk.Windows.RibbonControl ribControl = Autodesk.Windows.ComponentManager.Ribbon;
               
                var ribControl = new RibbonControl();

                var ribTab = new RibbonTab {Title = "Test", Id = "Test"};
                ribControl.Tabs.Add(ribTab);

                var ribSourcePanel = new RibbonPanelSource
                    {
                        Title = "My Tools",
                        DialogLauncher = new RibbonCommandItem {CommandHandler = new AdskCommandHandler()}
                    };

                //Add a Panel
		        var ribPanel = new RibbonPanel {Source = ribSourcePanel};
                ribTab.Panels.Add(ribPanel);

		        //Create button
		        var ribButton1 = new RibbonButton
		            {
		                Text = "Line" + "\n" + "Generator",
		                CommandParameter = "Line ",
		                ShowText = true,
		                LargeImage = Images.GetBitmap((Bitmap) _resourceManager.GetObject("LineImage")),
		                Image = Images.GetBitmap((Bitmap) _resourceManager.GetObject("LineImage")),
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

    public class AdskCommandHandler : System.Windows.Input.ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }
        event EventHandler System.Windows.Input.ICommand.CanExecuteChanged
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

    public class Images
    {
        public static BitmapImage GetBitmap(Bitmap image)
        {
            var stream = new MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = stream;
            bmp.EndInit();

            return bmp;
        }
    }
}
