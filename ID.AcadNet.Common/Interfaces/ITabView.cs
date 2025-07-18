using System.Drawing;
using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ITabView
    {
        int TabIndex { get; set; }
        string Name { get; set; }
        Size MinimumSize { get; set; }
        Size MaximumSize { get; set; }
        PaletteViewStatus Status { get; set; }
        StateEventIndex TabState { get; set; }
        Palette ParentPalette { get; set; }
        object ParentPaletteSet { get; set; }
        bool IsActive { get; set; }
        bool Current { get; set; }
        string Title { get; set; }
        int UniId { get; set; }
        bool Visible { get; set; }
        bool Complete { get; set; }
        string Comment { get; set; }
        double Width { get; set; }
        Size Size { get; set; }
        CommandArgs ActivateArgument { get; set; }
        object FindName(string datagrid);
        object DataContext { get; set; }

        //Methods
        void Apply();
        void Refresh(bool flagManualChange = false);

        //Events
        void OnActivate(CommandArgs argument = null);
        void OnDeactivate();
    }

    //public interface ITabSearchTextView : ITabView
    //{
    //    void Search(string text);
    //}
}