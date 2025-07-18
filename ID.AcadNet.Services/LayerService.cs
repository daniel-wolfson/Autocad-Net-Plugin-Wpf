using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.LayerManager;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Resources.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    public class LayerService : CollectionBase, ILayerService
    {
        private Document _doc => acadApp.DocumentManager.MdiActiveDocument;
        private Editor _ed => _doc.Editor;
        private Database _db => _doc.Database;
        private ObservableCollection<AcadLayer> _listItems = new ObservableCollection<AcadLayer>();

        public System.Windows.Forms.Form LayerFilterForm;
        public Type LayerFilterType;

        public LayerService()
        {
        }

        public AcadLayer this[int index] //public string Name(int index)
        {
            get { return _listItems[index]; }
            set { _listItems[index] = value; }
        }

        public AcadLayer Get(string layerName)
        {
            return _listItems.FirstOrDefault(x => x.LayerName == layerName);
        }

        public void Active(string layerName)
        {
            using (var tr = _db.TransactionManager.StartTransaction())
            {
                using (_doc.LockDocument())
                {
                    var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);
                    if (lt.Has(layerName))
                    {
                        LayerTableRecord ltr = (LayerTableRecord)tr.GetObject(lt[layerName], OpenMode.ForWrite);
                        _db.Clayer = lt[layerName];
                        Colors.Current = ltr.Color;
                        ltr.IsOff = false;
                        tr.Commit();
                    }
                }
            }
        }

        /// <summary> Add layer </summary>
        public void Add(string layerName, Color tColor)
        {
            LayerTableRecord ltr = null;
            try
            {
                using (var tr = _db.TransactionManager.StartTransaction())
                {
                    var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);
                    SymbolUtilityServices.ValidateSymbolName(layerName, false);

                    if (!lt.Has(layerName))
                    {
                        using (_doc.LockDocument())
                        {
                            ltr = new LayerTableRecord { Color = tColor, Name = layerName };
                            //Color.FromColorIndex(ColorMethod.ByAci, tColor)
                            lt.UpgradeOpen();
                            var ltId = lt.Add(ltr);
                            tr.AddNewlyCreatedDBObject(ltr, true);

                            _db.Clayer = ltId;
                            //value = ltr.Name
                            tr.Commit();
                            _ed.Regen();

                            _listItems.Add(new AcadLayer(ltr.Handle.ToString(), ltr.Name, ltr.Color.ColorIndex));
                        }
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Plugin.Logger.Error($"{nameof(LayerService)}.{nameof(Add)} error: ", ex);
            }
        }

        /// <summary> Add layer </summary>
        public void Add(string layerName, short tColor)
        {
            Add(layerName, Color.FromColorIndex(ColorMethod.ByAci, tColor));
        }

        /// <summary> Add layer </summary>
        public void Add(string value)
        {
            Add(value, Colors.White);
        }

        /// <summary> Add layers </summary>
        public void AddRange(string[] listNames, short[] tListColorsIndex)
        {
            if (listNames.Length > 0 & listNames.Length == tListColorsIndex.Length)
            {
                AddRange(listNames, Array.ConvertAll(tListColorsIndex, x => Color.FromColorIndex(ColorMethod.ByAci, x)));
            }
        }

        /// <summary> Add layers </summary>
        public void AddRange(string[] listNames, Color[] tListColors)
        {
            if (listNames.Length > 0 & listNames.Length == tListColors.Length)
            {
                for (var i = 0; i <= listNames.Length - 1; i++)
                {
                    Add(listNames[i], tListColors[i]);
                }
            }
        }

        /// <summary> Removing all draw entities from layer </summary>
        public void Clean(string layerName)
        {
            _db.XRemoveObjects(new[] { layerName });
        }

        /// <summary> Clean all layers </summary>
        public void CleanAll(string[] layerNames = null)
        {
            _db.XRemoveObjects(layerNames);
        }

        /// <summary> ContainsLayer </summary>
        public bool Contains(string layerName)
        {
            return GetAll().Select(x => x.LayerName).Contains(layerName);
        }

        /// <summary> GisLayerName </summary>
        public string GetWorkLayerName(string layerName = null)
        {
            layerName = layerName ?? Settings.Default.WorkLayerName;
            if (!Contains(layerName))
                Add(layerName, Colors.Black);
            return layerName;
        }

        /// <summary> Removing all draw entities from layer </summary>
        public void Remove(string layerName)
        {
            try
            {
                using (var tr = _db.TransactionManager.StartTransaction())
                {
                    var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);
                    if (lt.Has(layerName))
                    {
                        var ids = new ObjectIdCollection { lt[layerName] };
                        _db.Purge(ids);

                        if (ids.Count > 0)
                        {
                            var ltr = (LayerTableRecord)tr.GetObject(ids[0], OpenMode.ForWrite);
                            ltr.Erase(true);
                            //' Erase the unreferenced layer
                            tr.Commit();
                        }
                    }
                }

            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Plugin.Logger.Error($"{nameof(LayerService)}.{nameof(Remove)} error: ", ex);
            }
        }

        /// <summary> load layers from db </summary>
        public void Load()
        {
            _listItems.Clear();

            using (Transaction tr = _db.TransactionManager.StartOpenCloseTransaction())
            {
                var lt = tr.GetObject(_db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (lt != null)
                    foreach (ObjectId layerId in lt)
                    {
                        LayerTableRecord layer = tr.GetObject(layerId, OpenMode.ForRead) as LayerTableRecord;
                        if (layer != null && !layer.IsOff)
                        {
                            _listItems.Add(new AcadLayer(layer.Handle.ToString(), layer.Name, layer.Color.ColorIndex));
                        }
                    }
            }
        }

        /// <summary> create from exists rules </summary>
        public void Create(List<IRule> rules)
        {
            var arrRules = rules.Select(x => x.LayerDestination).ToArray();
            CleanAll(arrRules);
            var layerImets = rules.Select(r => new AcadLayer("", r.LayerDestination, (short)r.ColorIndex))
                     .Distinct();
            Create(new ObservableCollection<AcadLayer>(layerImets));
        }

        /// <summary> create from exists listItems </summary>
        public void Create(ObservableCollection<AcadLayer> listItems = null)
        {
            _listItems = listItems ?? new ObservableCollection<AcadLayer>();

            foreach (var item in _listItems)
            {
                using (var tr = _db.TransactionManager.StartTransaction())
                {
                    var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);
                    if (!lt.Has(item.LayerName))
                    {
                        using (_doc.LockDocument())
                        {
                            var ltr = new LayerTableRecord
                            {
                                Name = item.LayerName,
                                Color = Colors.GetColorFromIndex(item.ColorIndex)
                            };
                            lt.UpgradeOpen();
                            var ltId = lt.Add(ltr);
                            tr.AddNewlyCreatedDBObject(ltr, true);
                            lt.DowngradeOpen();
                            _db.Clayer = ltId;
                        }
                    }
                    tr.Commit();
                }
            }
        }

        /// <summary> Get all layers </summary>
        public ObservableCollection<AcadLayer> GetAll()
        {
            return _listItems;
        }

        /// <summary> Get current layer </summary>
        public AcadLayer Current()
        {
            string layerName;
            if (_db.Clayer != ObjectId.Null)
                using (var tr = _db.TransactionManager.StartTransaction())
                {
                    var ltr = (LayerTableRecord)tr.GetObject(_db.Clayer, OpenMode.ForRead);
                    layerName = ltr.Name;
                }
            else
                layerName = "0";
            return _listItems.FirstOrDefault(x => x.LayerName == layerName);
        }

        /// <summary> Set freeze to current layer </summary>
        public void Freeze(string layerName, bool freeze)
        {
            using (var tr = _db.TransactionManager.StartTransaction())
            {
                var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);
                if (lt.Has(layerName))
                {
                    using (_doc.LockDocument())
                    {
                        var ltr = (LayerTableRecord)tr.GetObject(lt[layerName], OpenMode.ForWrite);
                        //Db.Clayer = lt[layerName];
                        //Colors.Current = ltr.Color;
                        ltr.IsFrozen = freeze;
                        tr.Commit();
                    }
                }
            }
        }

        public bool IsOnOff(string layerName)
        {
            var result = false;
            using (var tr = _db.TransactionManager.StartTransaction())
            {
                var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);
                if (lt.Has(layerName))
                {
                    var ltr = (LayerTableRecord)tr.GetObject(lt[layerName], OpenMode.ForRead);
                    result = !ltr.IsOff;
                }
            }
            return result;
        }

        public void OnOff(string layerName, bool tOnOff)
        {
            using (var tr = _db.TransactionManager.StartTransaction())
            {
                using (_doc.LockDocument())
                {
                    var lt = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);
                    if (lt.Has(layerName))
                    {
                        var ltr = (LayerTableRecord)tr.GetObject(lt[layerName], OpenMode.ForWrite);
                        _db.Clayer = lt[layerName];
                        Colors.Current = ltr.Color;
                        ltr.IsOff = !tOnOff;
                        tr.Commit();
                    }
                }
            }
        }

        public void ListLayerFilters()
        {
            // List the nested layer filters
            var lfc = _db.LayerFilters.Root.NestedFilters;

            for (int i = 0; i < lfc.Count; i++)
            {
                var lf = lfc[i];
                _ed.WriteMessage("\n{0} - {1} (can{2} be deleted)", i + 1, lf.Name, (lf.AllowDelete ? "" : "not"));
            }
        }

        public void CreateLayerFilters()
        {
            try
            {
                // Get the existing layer filters
                // (we will add to them and set them back)
                var lft = _db.LayerFilters;
                var lfc = lft.Root.NestedFilters;

                // Create three new layer filters
                var lf1 = new LayerFilter { Name = "Unlocked Layers", FilterExpression = "LOCKED==\"False\"" };
                var lf2 = new LayerFilter { Name = "White Layers", FilterExpression = "COLOR==\"7\"" };
                var lf3 = new LayerFilter { Name = "Visible Layers", FilterExpression = "OFF==\"False\" AND FROZEN==\"False\"" };

                // Add them to the collection
                lfc.Add(lf1);
                lfc.Add(lf2);
                lfc.Add(lf3);

                // Set them back on the Database
                _db.LayerFilters = lft;

                // List the layer filters, to see the new ones
                ListLayerFilters();
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(LayerService)}.{nameof(CreateLayerFilters)} error: ", ex);
            }
        }

        public void DeleteLayerFilter()
        {
            ListLayerFilters();
            try
            {
                // Get the existing layer filters
                // (we will add to them and set them back)

                var lft = _db.LayerFilters;
                var lfc = lft.Root.NestedFilters;

                // Prompt for the index of the filter to delete
                var pio = new PromptIntegerOptions("\n\nEnter index of filter to delete") { LowerLimit = 1, UpperLimit = lfc.Count };

                PromptIntegerResult pir = _ed.GetInteger(pio);

                if (pir.Status != PromptStatus.OK) return;

                // Get the selected filter
                LayerFilter lf = lfc[pir.Value - 1];

                // If it's possible to delete it, do so
                if (!lf.AllowDelete)
                {
                    _ed.WriteMessage("\nLayer filter cannot be deleted.");
                }

                else
                {
                    lfc.Remove(lf);
                    _db.LayerFilters = lft;
                    ListLayerFilters();
                }
            }

            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(LayerService)}.{nameof(DeleteLayerFilter)} error: ", ex);
            }
        }

        public void ChangeLayerNamesDynamically()
        {
            dynamic layers = HostApplicationServices.WorkingDatabase.LayerTableId;

            foreach (dynamic l in layers)
                if (l.Name != "0")
                    l.Name = "First Floor " + l.Name;
        }
    }
}

//public List<RuleArgs> Rules = new List<RuleArgs>();
//public void AddRule(RuleArgs ruleArgs)
//{
//    Rules.Add(ruleArgs);
//}
//public void AddRule(string ruleName, string objectTypePattern, int layerTypeId, string layerOnPattern, string layerOffPattern, short drawColorIndex, LineTypes drawTypeLine)
//{
//    AddRule(ruleName, objectTypePattern, layerTypeId, layerOnPattern, layerOffPattern, drawColorIndex, drawTypeLine.ToString());
//}
//public void AddRule(string ruleName, string objectTypePattern, int layerTypeId, string layerOnPattern, string layerOffPattern, short drawColorIndex, string drawTypeLine)
//{
//    Rules.Add(new RuleArgs
//    {
//        RuleNamePlace = ruleName,
//        LayerTypeId = layerTypeId,
//        LayerOnPattern = layerOnPattern,
//        LayerOffPattern = layerOffPattern,
//        ColorIndex = drawColorIndex,
//        ObjectPattern = objectTypePattern,
//        TypeLineId = Tools.GetLineTypeId(drawTypeLine),
//        TranformMode = false
//    });
//    ListItems.Add(new LayerItem() { Color = Color.FromColorIndex(ColorMethod.ByAci, drawColorIndex), Name = ruleName });
//}
//public void AddRangeRule(RuleArgs[] filterSet)
//{
//    foreach (var layerType in filterSet)
//    {
//        Rules.Add(layerType);
//    }
//}
//public List<RuleArgs> GetRulesById(int typeId)
//{
//    return Rules.Where(X => X.LayerTypeId == typeId).ToList();
//}
//public RuleArgs GetRelevantRule(string layerName)
//{
//    return Rules.Find(r =>
//        (r.LayerOnPattern.Length == 1
//                ? (layerName == r.LayerOnPattern && layerName != r.LayerOffPattern)
//                : (layerName.Contains(r.LayerOnPattern) && !layerName.Contains(r.LayerOffPattern))
//        ));
//}