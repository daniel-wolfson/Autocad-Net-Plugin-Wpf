using Autodesk.AutoCAD.Windows;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Properties;
using Serilog;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Media;

namespace Intellidesk.AcadNet.Common.Core
{
    public class PaletteTabCollection : CollectionBase, IPaletteTabCollection
    {
        private readonly string _paletteName = ToolsManager.PluginSettings.Name + ".Tools";

        public bool IsLoaded { get; set; }

        private PaletteSetSizeEventHandler _sizeChanged;
        //private PaletteSetStateEventHandler _onRootPaletteSetStateChanged;
        public event PaletteSetStateEventHandler PaletteSetChanged;

        public PluginPaletteSet RootPaletteSet { get; set; }

        public int CurrentTabIndex { get; set; }

        #region <ctor>

        public PaletteTabCollection()
        {
            var guid = new Guid(Settings.Default.PaletteSetId);
            RootPaletteSet = new PluginPaletteSet();
            RootPaletteSet.PaletteActivated += OnPaletteActivated;
            RootPaletteSet.StateChanged += OnStateChanged;
            RootPaletteSet.PaletteSetDestroy += OnPaletteSetDestroy;

            _sizeChanged = (sender, args) =>
            {
                if (IsLoaded)
                    ToolsManager.PluginSettings.ToolPanelWidth = args.Width; // Convert.ToInt32(args.DeviceIndependentWidth);
            };
            RootPaletteSet.SizeChanged += _sizeChanged;

            RootPaletteSet.Load += (sender, args) => { IsLoaded = true; };
        }

        #endregion

        #region <properties>

        //private IDictionary<string, IPanelTabView> Tabs
        //{
        //    get
        //    {
        //        return List.Cast<IPanelTabView>().ToDictionary(x => x.Name, x => x);
        //    }
        //}

        //public ICollection<string> Keys => Tabs.Keys;

        //public IPanelTabView this[int index]
        //{
        //    get { return (IPanelTabView)List[index]; }
        //    set
        //    {
        //        if (List.Contains(value))
        //            List[index] = value;
        //        else
        //            AddTab(value);
        //    }
        //}

        public IPanelTabView this[string key]
        {
            get
            {
                if (List.Cast<IPanelTabView>().All(x => x.Name != key))
                    return null;
                return List.Cast<IPanelTabView>().FirstOrDefault(x => x.Name == key);
            }
            set
            {
                if (List.Cast<IPanelTabView>().Any(x => x.Name == key))
                    List[List.IndexOf(List.Cast<IPanelTabView>().FirstOrDefault(x => x.Name == key))] = value;
                else
                    AddTab(value);
            }
        }

        public IPanelTabView this[PaletteNames key]
        {
            get { return this[key.ToString()]; }
            set
            {
                this[key.ToString()] = value;
            }
        }

        public IPanelTabView Current => List.Count != 0
            ? (IPanelTabView)List[CurrentTabIndex] : null;

        #endregion

        #region <methods>

        public int AddTab(IPanelTabView value, bool tReplaceTab = false)
        {
            if (List.IndexOf(value) > 0) return value.TabIndex;

            //Resources.Properties.Settings.Default.Upgrade();
            //Resources.Properties.Settings.Default.ToolPanelTop = value.Name;
            //Resources.Properties.Settings.Default.Save();

            List.Add(value);
            value.TabIndex = List.IndexOf(value);

            try
            {
                Visual control = (Visual)value;
                Palette p = RootPaletteSet.AddVisual(value.Name, control);
                value.ParentPalette = p;
                value.ParentPaletteSet = RootPaletteSet;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                // ignored
            }

            //RootPaletteSet.KeepFocus = keepFocus;
            //RootPaletteSet.Visible = true;
            //GridLinePosition(value.PalleteControl, 0.4);
            ////value.PalleteControl.pgrid.SelectedObject = value
            ////value.PalleteControl.pgrid.PropertySort = Windows.Forms.PropertySort.Categorized
            return value.TabIndex;
        }

        public bool Contains(IPanelTabView value)
        {
            // If value is not of type Int16, this will return false.
            return List.Contains(value);
        }

        public bool ContainsTab(PaletteNames paletteName)
        {
            return List.Cast<IPanelTabView>().Any(x => x.Name.ToLower() == paletteName.ToString().ToLower());
        }

        public int IndexOf(PaletteNames paletteName)
        {
            var tab = List.Cast<IPanelTabView>().FirstOrDefault(x => x.Name == paletteName.ToString());
            if (tab != null)
                return tab.TabIndex;
            return -1;
        }

        public int IndexOf(IPaletteSet value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, IPaletteSet value)
        {
            List.Insert(index, value);
        }

        public void Activate(PaletteNames paletteTabName, ICommandArgs activateArgument = null, Action callBack = null)
        {
            IPanelTabView paletteTab = this[paletteTabName];
            Activate(paletteTab, activateArgument, callBack);
        }

        public void Activate(IPanelTabView paletteTab, ICommandArgs activateArgument = null, Action callBack = null)
        {
            if (callBack != null)
            {
                PalettePersistEventHandler start = null;
                start = (sender, e) =>
                {
                    RootPaletteSet.Load -= start;
                    callBack();
                };
                RootPaletteSet.Load += start;
            }

            paletteTab.ActivateArgument = activateArgument;

            if (paletteTab.TabIndex == CurrentTabIndex)
                OnPaletteActivated(null, new PaletteActivatedEventArgs(paletteTab.ParentPalette, null));

            RootPaletteSet.Activate(paletteTab.TabIndex);
        }

        public void OnPaletteActivated(object sender, PaletteActivatedEventArgs e)
        {
            if (e.Deactivated != null)
            {
                var tab = List.Cast<IPanelTabView>().FirstOrDefault(x => x.Name == e.Deactivated.Name);
                if (tab != null)
                {
                    tab.IsActive = false;
                    tab.OnDeactivate();
                    if (tab.DataContext != null)
                        ((BaseViewModel)tab.DataContext).IsActive = false;
                }

                if (List.Count == 1) //&& _onRootPaletteSetStateChanged != null
                {
                    //RootPaletteSet.StateChanged -= OnStateChanged;
                }
            }
            else
            {
                List.Cast<IPanelTabView>().ForEach(x => x.IsActive = false);
            }

            if (e.Activated != null)
            {
                IPanelTabView tab = List.Cast<IPanelTabView>().FirstOrDefault(x => x.Name == e.Activated.Name);
                if (tab != null)
                {
                    CurrentTabIndex = tab.TabIndex;
                    tab.IsActive = true;
                    ToolsManager.PluginSettings.TabIndex = tab.TabIndex;

                    tab.OnActivate(tab.ActivateArgument);
                    tab.ActivateArgument = null;

                    if (tab.DataContext != null)
                        ((BaseViewModel)tab.DataContext).IsActive = true; //(BaseViewModel<ElementDefinition>)
                }
            }
        }

        public void OnPaletteSetDestroy(object sender, EventArgs e)
        {
            //if (_onRootPaletteSetStateChanged != null)
            //    foreach (var d in _onRootPaletteSetStateChanged.GetInvocationList())
            //        _onRootPaletteSetStateChanged -= (PaletteSetStateEventHandler)d;
            var paletteSet = sender as PaletteSet;

            if (paletteSet != null)
                foreach (IPanelTabView item in List)
                {
                    item.TabState = StateEventIndex.Hide;
                    item.OnDeactivate();
                }

            if (e == EventArgs.Empty)
            {
                List.Clear();
                RootPaletteSet = null;
            }

            if (paletteSet != null)
            {
                //paletteSet.StateChanged -= OnStateChanged;
                paletteSet.PaletteSetDestroy -= OnPaletteSetDestroy;
                paletteSet.Dispose();
            }
        }

        private void OnStateChanged(object sender, PaletteSetStateEventArgs args)
        {
            var ps = sender as PaletteSet;
            switch (args.NewState)
            {
                case StateEventIndex.Hide:
                    foreach (IPanelTabView tab in List)
                    {
                        tab.TabState = StateEventIndex.Hide;
                        if (tab.IsActive)
                            tab.OnDeactivate();
                    }
                    break;

                case StateEventIndex.Show:
                    if (ps != null && ps.Visible)
                        foreach (IPanelTabView tab in List)
                        {
                            tab.TabState = StateEventIndex.Show;
                            if (!tab.IsActive)
                                tab.OnActivate();
                        }
                    break;
            }
            if (PaletteSetChanged != null)
                PaletteSetChanged(RootPaletteSet, new PaletteSetStateEventArgs(args.NewState));
        }

        public void GridLinePosition(object tInnerControl, double value)
        {
            //if (tInnerControl != null) {
            //    Type propGridType = tInnerControl.pGrid.GetType;
            //    FieldInfo fi = propGridType.GetField("gridView", BindingFlags.NonPublic | BindingFlags.Instance);
            //    object gridViewRef = fi.GetValue(tInnerControl.pGrid);
            //    Type gridViewType = gridViewRef.GetType();
            //    //MoveSplitterTo-Method  PropertyGridViews
            //    MethodInfo mi = gridViewType.GetMethod("MoveSplitterTo", BindingFlags.NonPublic | BindingFlags.Instance);
            //    int gridColWidth = Convert.ToInt32(tInnerControl.pGrid.Width * value);
            //    //30% from PropertyGrids 
            //    // = PropertyGridView.MoveSplitterTo(30%) 
            //    mi.Invoke(gridViewRef, new object[] { gridColWidth });
            //}
        }

        public void CloseTab(int index)
        {
            var item = List[index] as IPanelTabView;
            if (item != null)
                Remove(item);
        }
        public void CloseTab(PaletteNames paletteName, bool removeAfterClose = true)
        {
            CloseTab((int)paletteName);
        }

        public void CloseTab(IPanelTabView tab, bool removeAfterClose = true)
        {
            if (tab == null) return;

            tab.OnDeactivate();
            if (removeAfterClose)
                Remove(tab);
            else
            {
                tab.OnDeactivate();
                tab.Visible = false;
            }
        }

        public int AddTabCnt(int tCntId, bool tReplaceTab = false)
        {
            //var _with1 = Building.Floors(Building.Floors.Current).Constructions(tCntId);
            //_with1.TabIndex = List.Count;
            //_with1.PalleteControl = new CntDetailControl(tCntId);
            ////With DirectCast(.PalleteControl, CntDetailControl)
            ////    .pGrid.SelectedObject = Building.Floors(Building.Floors.Current).Constructions(tCntId)
            ////    .pGrid.PropertySort = Windows.Forms.PropertySort.Categorized
            ////End With
            //RootPaletteSet.Add(_with1.Header, _with1.PalleteControl);
            //GridLinePosition(_with1.PalleteControl, 0.5);
            //if (!PaletteIndex.Contains(_with1.Header))
            //    PaletteIndex.Add(_with1.Header, _with1.TabIndex);
            return 0; //List.Add(Building.Floors(Building.Floors.Current).Constructions(tCntId));
        }

        public int AddTabFloor(int tFloorId, bool tReplaceTab = false)
        {
            //var _with2 = Building.Floors(tFloorId);
            //_with2.TabIndex = List.Count;
            //_with2.PalleteControl = new FloorControl(Building.Floors(tFloorId).Constructions.CntRootNodes.ToArray, tFloorId);
            ////With DirectCast(Building.Floors(tFloorId).PalleteControl, FloorControl)
            ////    '.CntTreeView.BeginUpdate()
            ////    ''.CntTreeView.Nodes.Clear()
            ////    ''For Each arr As CntTreeNode In Building.Floors(tFloorId).Constructions.CntRootNodes 'Constructions.CntRootNodes.ToArray()
            ////    ''    .CntTreeView.Nodes.Remove(arr)
            ////    ''    .CntTreeView.Nodes.AddRange({arr})
            ////    ''Next
            ////    '.CntTreeView.ImageList = CntCollection.CntImgList
            ////    '.CntTreeView.Refresh()
            ////    '.CntTreeView.EndUpdate()
            ////    .pGrid.SelectedObject = Building.Floors(tFloorId)
            ////    .pGrid.PropertySort = Windows.Forms.PropertySort.Categorized
            ////End With
            //RootPaletteSet.Add(_with2.Header, _with2.PalleteControl);
            //GridLinePosition(_with2.PalleteControl, 0.5);
            //if (!PaletteIndex.Contains(_with2.Header))
            //    PaletteIndex.Add(_with2.Header, _with2.TabIndex);
            //return List.Add(Building.Floors(tFloorId));
            return 0;
        }

        public void Remove(IPanelTabView value)
        {
            List.Remove(value);
        }

        public void RemoveAllEvents()
        {
            if (RootPaletteSet != null)
            {
                RootPaletteSet.PaletteActivated -= OnPaletteActivated;
                try
                {
                    RootPaletteSet.StateChanged -= OnStateChanged;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                }
                RootPaletteSet.PaletteSetDestroy -= OnPaletteSetDestroy;
                RootPaletteSet.SizeChanged -= _sizeChanged;
                RootPaletteSet.Close();
            }
        }

        #endregion

        #region <overrided methods>

        protected override void OnInsert(int index, object value)
        {
            if (IsLoaded)
                ((IPanelTabView)value).Visible = true;
        }

        protected override void OnRemove(int index, object value)
        {
            // Insert additional code to be run only when removing values. 
            var tab = (IPanelTabView)value;
            tab.OnDeactivate();
            tab.Visible = false;
            // foreach (var palette in RootPaletteSet.Cast<IPanelTabView>().Where(x => x.Name == tab.Name))
            try
            {
                RootPaletteSet.Remove(index);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                // ignored
            }
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            // Insert additional code to be run only when setting values.
            var a = oldValue;
            var b = newValue;
        }

        protected override void OnValidate(object value)
        {
            if (!(value is IPanelTabView))
            {
                throw new ArgumentException("value must be of type IPanelTabView.", "value");
            }
        }

        #endregion


    }
}
