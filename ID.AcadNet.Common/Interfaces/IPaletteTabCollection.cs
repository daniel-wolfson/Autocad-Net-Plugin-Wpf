using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Enums;
using System;
using System.Collections;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface IPaletteTabCollection
    {
        event PaletteSetStateEventHandler PaletteSetChanged;
        //IPanelTabView this[int index] { get; set; }
        IPanelTabView this[string key] { get; set; }
        IPanelTabView this[PaletteNames key] { get; set; }

        IPanelTabView Current { get; }
        int Capacity { get; set; }
        int Count { get; }
        PluginPaletteSet RootPaletteSet { get; set; }
        int AddTab(IPanelTabView value, bool tReplaceTab = false);
        int AddTabCnt(int tCntId, bool tReplaceTab = false);
        int AddTabFloor(int tFloorId, bool tReplaceTab = false);
        bool ContainsTab(PaletteNames tabName);
        int IndexOf(IPaletteSet value);
        void Insert(int index, IPaletteSet value);
        void CloseTab(int index);
        void CloseTab(PaletteNames paletteName, bool removeAfterClose = true);
        void CloseTab(IPanelTabView value, bool removeAfterClose = true);
        void Remove(IPanelTabView value);
        void RemoveAllEvents();
        bool Contains(IPanelTabView value);
        //void Activate(int index);
        void OnPaletteActivated(object sender, PaletteActivatedEventArgs e);
        void OnPaletteSetDestroy(object sender, EventArgs e);
        void GridLinePosition(object tInnerControl, double value);
        void Clear();
        //ICollection<string> Keys { get; }
        void RemoveAt(int index);
        IEnumerator GetEnumerator();
        void Activate(PaletteNames paletteTabName, ICommandArgs activateArgument = null, Action action = null);
        void Activate(IPanelTabView paletteTab, ICommandArgs activateArgument = null, Action action = null);
        int CurrentTabIndex { get; set; }
        bool IsLoaded { get; set; }
    }
}