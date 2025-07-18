using System;
using System.Drawing;
using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Common.Enums;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface IPaletteView
    {
        byte TabId { get; set; }
        string Name { get; set; }

        Size SizeMin { get; set; }
        Size SizeMax { get; set; }
        PaletteViewStatus Status { get; set; }
        Autodesk.AutoCAD.Windows.StateEventIndex PaletteState { get; set; }
        PaletteSet PaletteSetParent { get; set; }


        bool Current { get; set; }
        string Header { get; set; }
        object ParentControl { get; set; }
        object PalleteControl { get; set; }
        bool Complete { get; set; }
        string Comment { get; set; }
        int UniId { get; set; }
        int Width { get; set; }
        bool Visible { get; set; }

        void OnActivate(object tObj = null);
        void OnDeactivate();
        void Refresh(bool flagManualChange = false);
        void Apply();

        void OnPropertyChanged(object sender, EventArgs e);
        bool SwitchSizeMode { get; set; }
    }
}