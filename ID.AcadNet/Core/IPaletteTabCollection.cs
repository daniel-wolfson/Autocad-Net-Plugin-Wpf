using System;
using System.Collections;
using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Interfaces;

namespace Intellidesk.AcadNet.Core
{
    public interface IPaletteTabCollection
    {
        event PaletteSetStateEventHandler PaletteSetChanged;
        ITabView this[int index] { get; set; }
        ITabView this[string key] { get; set; }
        ITabView Current { get; }

        int Capacity { get; set; }
        int Count { get; }
        PaletteSet RootPaletteSet { get; set; }
        int AddTab(ITabView value, bool tReplaceTab = false);
        int AddTabCnt(int tCntId, bool tReplaceTab = false);
        int AddTabFloor(int tFloorId, bool tReplaceTab = false);
        int IndexOf(IPaletteSet value);
        void Insert(int index, IPaletteSet value);
        void CloseTab(int index);
        void Remove(ITabView value);
        bool Contains(ITabView value);
        //void Activate(int index);
        void OnActivate(object sender, PaletteActivatedEventArgs e);
        void OnDeactivate(object sender, EventArgs e);
        void GridLinePosition(object tInnerControl, double value);
        void Clear();
        void RemoveAt(int index);
        IEnumerator GetEnumerator();
    }
}