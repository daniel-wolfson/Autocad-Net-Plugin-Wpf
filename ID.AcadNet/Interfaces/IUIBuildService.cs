using Autodesk.AutoCAD.Customization;
using Autodesk.Windows;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.ViewModels;
using System;
using System.Resources;
using RibbonControl = Autodesk.Windows.RibbonControl;
using RibbonPanelSource = Autodesk.AutoCAD.Customization.RibbonPanelSource;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IUIBuildService1
    {
        IPaletteTabCollection PaletteTabs { get; set; }
        MapViewModel MainViewModel { get; set; }
        ResourceManager GetResourceManager();
        CustomizationSection GetCustomizationSection();
        RibbonPanelSource AddRibbonPanel(CustomizationSection cs, string tabName, string panelName);
        RibbonControl AddRibbonTab();
        void AddRibbonButton();
        void AddRibbonTextBox(RibbonTab rt, string title);
        void AcadWindowSetFocus();
        void WriteMessage(string s);
        void Alert(string alert);
        void PaletteTabClose(object ps, EventArgs empty);
    }
}