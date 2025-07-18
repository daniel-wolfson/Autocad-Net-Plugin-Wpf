using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Common.Enums;
using System.Drawing;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IPanelTabView
    {
        int TabIndex { get; set; }
        string Name { get; set; }
        Size MinimumSize { get; set; }
        Size MaximumSize { get; set; }
        PaletteViewStatus Status { get; set; }
        StateEventIndex TabState { get; set; }
        Palette ParentPalette { get; set; }
        object ParentPaletteSet { get; set; }
        PaletteNames PanelTabName { get; }
        bool IsLive { get; set; }
        bool IsActive { get; set; }
        string Title { get; set; }
        bool Complete { get; set; }
        string Comment { get; set; }
        double Width { get; set; }
        Size Size { get; set; }
        ICommandArgs ActivateArgument { get; set; }
        object FindName(string datagrid);
        object DataContext { get; set; }
        bool Visible { get; set; }
        //Methods
        void Apply();
        void Refresh(bool flagManualChange = false);

        //Events
        void OnActivate(ICommandArgs argument = null);
        void OnDeactivate();
    }
}