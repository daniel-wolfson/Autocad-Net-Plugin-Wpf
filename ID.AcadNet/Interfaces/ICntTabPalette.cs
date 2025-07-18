using System.ComponentModel;

namespace Intellidesk.AcadNet.Interfaces
{
    public interface ICntTabPalette
    {
        bool Current { get; set; }
        string Name { get; set; }
        string Header { get; set; }
        object Parent { get; set; }
        object PalleteControl { get; set; }
        int UniId { get; set; }
        int TabId { get; set; }
        bool Visible { get; set; }
        bool Complete { get; set; }
        void OnActivate(object tObj = null);
        void OnDeactivate();
        //Sub OnDelete()
        void Refresh(bool flagManualChange = false);
        void Apply();
        [Browsable(true)]
        string Comment { get; set; }
    }
}