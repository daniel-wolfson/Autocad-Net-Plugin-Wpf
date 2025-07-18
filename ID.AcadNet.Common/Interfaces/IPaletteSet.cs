using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Common.Enums;
using System;
using System.Drawing;
using System.Windows.Media;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IPaletteSet
    {
        event EventHandler PaletteSetClosed;
        event PaletteActivatedEventHandler PaletteActivated;
        event PaletteSetStateEventHandler StateChanged;
        //UserControl CurrentView { get; set; }
        PaletteState State { get; set; }
        DockSides Dock { get; set; }
        //bool FullView { get; set; }
        string Name { get; set; }
        bool KeepFocus { get; set; }
        bool Visible { get; set; }

        Size Size { get; set; }
        Size MinimumSize { get; set; }
        Size MaximumSize { get; set; }

        void OnStateChanged(object sender, PaletteSetStateEventArgs args);
        void Remove(int index);
        void Activate(int index);
        Palette AddVisual(string name, Visual control);
        void OnDeactivate();
        void OnActivate();
    }
}