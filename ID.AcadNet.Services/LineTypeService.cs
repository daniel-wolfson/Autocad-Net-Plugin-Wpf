using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.LayerManager;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.Data.Models.Cad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services
{
    //public interface ILineTypeService<T> : IService<T> where T : LineTypeItem, new()
    //{
    //}

    //[ComponentModel.DefaultProperty("Name")]

    public class LineTypeService : CollectionBase, ILineTypeService
    {
        private readonly IUnityContainer _unityContainer;
        private Editor ed { get { return doc.Editor; } }
        private Database db { get { return doc.Database; } }
        private Document doc { get { return App.DocumentManager.MdiActiveDocument; } }

        private List<LineTypeItem> ListItems = new List<LineTypeItem>();
        public List<string> ListNames = new List<string>();
        public List<Color> ListColors = new List<Color>();

        //public ArrayList ObjectIds = new ArrayList();
        public System.Windows.Forms.Form LayerFilterForm;

        public Type LayerFilterType;

        public LineTypeService(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public void Active(string value)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                using (doc.LockDocument())
                {
                    var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    if (lt.Has(value))
                    {
                        var ltr = (LayerTableRecord)tr.GetObject(lt[value], OpenMode.ForWrite);
                        db.Clayer = lt[value];
                        Colors.Current = ltr.Color;
                        ltr.IsOff = false;
                        tr.Commit();
                    }
                }
            }
        }

        /// <summary> Add layer </summary>
        public object Add(string value, Color tColor)
        {
            LayerTableRecord ltr = null;
            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    SymbolUtilityServices.ValidateSymbolName(value, false);
                    if (lt.Has(value) == false)
                    {
                        using (doc.LockDocument())
                        {
                            ltr = new LayerTableRecord { Color = tColor, Name = value };
                            //Color.FromColorIndex(ColorMethod.ByAci, tColor)
                            lt.UpgradeOpen();
                            var ltId = lt.Add(ltr);
                            tr.AddNewlyCreatedDBObject(ltr, true);
                            //ObjectIds.Add(ltr.ObjectId);

                            ListNames.Add(value);
                            ListColors.Add(tColor);

                            db.Clayer = ltId;
                            //value = ltr.Name
                            tr.Commit();
                            ed.Regen();
                        }
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Plugin.Logger.Error($"{nameof(LayerService)}.{nameof(Add)} error: ", ex);
                return null;
            }

            return ltr;
        }

        /// <summary> Add layer </summary>
        public object Add(string value, short tColor)
        {
            return Add(value, Color.FromColorIndex(ColorMethod.ByAci, tColor));
        }

        /// <summary> Add layer </summary>
        public object Add(string value)
        {
            return Add(value, Colors.White);
        }

        /// <summary> Add layers </summary>
        public void AddRange(string[] tListNames, short[] tListColorsIndex)
        {
            if (tListNames.Length > 0 & tListNames.Length == tListColorsIndex.Length)
            {
                AddRange(tListNames, Array.ConvertAll(tListColorsIndex, x => Color.FromColorIndex(ColorMethod.ByAci, x)));
            }
        }

        /// <summary> Add layers </summary>
        public void AddRange(string[] tListNames, Color[] tListColors)
        {
            if (tListNames.Length > 0 & tListNames.Length == tListColors.Length)
            {
                for (var i = 0; i <= tListNames.Length - 1; i++)
                {
                    Add(tListNames[i], tListColors[i]);
                }
            }
        }

        /// <summary> Removing all draw entities from layer </summary>
        public void Clean(string value)
        {
            db.XRemoveObjects(new[] { value });
        }

        /// <summary> Clean all layers </summary>
        public void CleanAll(string[] layerNames = null)
        {
            db.XRemoveObjects(layerNames);
        }

        /// <summary> ContainsLayer </summary>
        public bool Contains(string layerName)
        {
            return GetAll().Select(x => x.Name).Contains(layerName);
        }

        /// <summary> Removing all draw entities from layer </summary>
        public void Remove(string value)
        {
            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    if (lt.Has(value))
                    {
                        var ids = new ObjectIdCollection { lt[value] };
                        db.Purge(ids);
                        ListNames.RemoveAt(ListNames.IndexOf(value));
                        ListColors.RemoveAt(ListNames.IndexOf(value));

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
                Plugin.Logger.Error($"{nameof(LineTypeService)}.{nameof(Remove)} error: ", ex);
            }
        }

        public void Init(List<IRule> rules)
        {
            var arrRules = rules.Select(x => x.LayerDestination).ToArray();
            CleanAll(arrRules);
            var layerImets =
                rules.Select(r => new LineTypeItem
                { Color = Color.FromColorIndex(ColorMethod.ByAci, (short)r.ColorIndex), Name = r.LayerDestination })
                                    .Distinct()
                                    .ToList();
            Init(layerImets);
        }

        public void Init(List<Rule> rules)
        {
            throw new NotImplementedException();
        }

        public void Init(List<LineTypeItem> listItems = null)
        {
            var items = new List<LineTypeItem>();
            ListItems = listItems ?? items;
            foreach (var item in ListItems)
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    if (!lt.Has(item.Name))
                    {
                        using (doc.LockDocument())
                        {
                            var ltr = new LayerTableRecord { Name = item.Name, Color = item.Color };
                            lt.UpgradeOpen();
                            var ltId = lt.Add(ltr);
                            tr.AddNewlyCreatedDBObject(ltr, true);
                            lt.DowngradeOpen();
                            db.Clayer = ltId;
                        }
                    }
                    tr.Commit();
                }
            }
        }

        /// <summary> Get all layers </summary>
        public List<LineTypeItem> GetAll()
        {
            List<LineTypeItem> items = new List<LineTypeItem>();
            using (Transaction tr = db.TransactionManager.StartOpenCloseTransaction())
            {
                var lt = tr.GetObject(db.LinetypeTableId, OpenMode.ForRead) as LayerTable;
                if (lt != null)
                    foreach (ObjectId lineTypeId in lt)
                    {
                        LayerTableRecord layer = tr.GetObject(lineTypeId, OpenMode.ForRead) as LayerTableRecord;
                        if (layer != null && !layer.IsOff)
                            items.Add(new LineTypeItem
                            {
                                Name = layer.Name,
                                LineTypeId = lineTypeId,
                                Color = layer.Color
                            });
                    }
            }
            return items;
        }

        /// <summary> Get current layer </summary>
        public LineTypeItem Current()
        {
            LayerTableRecord retValue;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                retValue = (LayerTableRecord)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            }
            return ListItems.FirstOrDefault(x => x.Name == retValue.Name);
        }

        /// <summary> Set freeze to current layer </summary>
        public void Freeze(string value, bool tFreeze)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (lt.Has(value))
                {
                    using (doc.LockDocument())
                    {
                        var ltr = (LayerTableRecord)tr.GetObject(lt[value], OpenMode.ForWrite);
                        db.Clayer = lt[value];
                        Colors.Current = ltr.Color;
                        ltr.IsFrozen = tFreeze;
                        tr.Commit();
                    }
                }
            }
        }

        public bool IsOnOff(string value)
        {
            var result = false;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                if (lt.Has(value))
                {
                    var ltr = (LayerTableRecord)tr.GetObject(lt[value], OpenMode.ForRead);
                    result = !ltr.IsOff;
                }
            }
            return result;
        }

        public void OnOff(string value, bool tOnOff)
        {
            using (var tr = db.TransactionManager.StartTransaction())
            {
                using (doc.LockDocument())
                {
                    var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    if (lt.Has(value))
                    {
                        var ltr = (LayerTableRecord)tr.GetObject(lt[value], OpenMode.ForWrite);
                        db.Clayer = lt[value];
                        Colors.Current = ltr.Color;
                        ltr.IsOff = !tOnOff;
                        tr.Commit();
                    }
                }
            }
        }

        public string this[int index] //public string Name(int index)
        {
            get { return ListNames[index]; }
            set { ListNames[index] = value; }
        }

        public void ListLayerFilters()
        {
            // List the nested layer filters
            var lfc = db.LayerFilters.Root.NestedFilters;

            for (int i = 0; i < lfc.Count; i++)
            {
                var lf = lfc[i];
                ed.WriteMessage("\n{0} - {1} (can{2} be deleted)", i + 1, lf.Name, (lf.AllowDelete ? "" : "not"));
            }
        }

        public void CreateLayerFilters()
        {
            try
            {
                // Get the existing layer filters
                // (we will add to them and set them back)
                var lft = db.LayerFilters;
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
                db.LayerFilters = lft;

                // List the layer filters, to see the new ones
                ListLayerFilters();
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(LineTypeService)}.{nameof(CreateLayerFilters)} error: ", ex);
            }
        }

        public void DeleteLayerFilter()
        {
            ListLayerFilters();
            try
            {
                // Get the existing layer filters
                // (we will add to them and set them back)

                var lft = db.LayerFilters;
                var lfc = lft.Root.NestedFilters;

                // Prompt for the index of the filter to delete
                var pio = new PromptIntegerOptions("\n\nEnter index of filter to delete") { LowerLimit = 1, UpperLimit = lfc.Count };

                PromptIntegerResult pir = ed.GetInteger(pio);

                if (pir.Status != PromptStatus.OK) return;

                // Get the selected filter
                LayerFilter lf = lfc[pir.Value - 1];

                // If it's possible to delete it, do so
                if (!lf.AllowDelete)
                {
                    ed.WriteMessage("\nLayer filter cannot be deleted.");
                }

                else
                {
                    lfc.Remove(lf);
                    db.LayerFilters = lft;
                    ListLayerFilters();
                }
            }

            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(LineTypeService)}.{nameof(DeleteLayerFilter)} error: ", ex);
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