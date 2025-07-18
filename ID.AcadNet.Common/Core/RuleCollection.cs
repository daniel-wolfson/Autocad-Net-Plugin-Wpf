using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Interfaces;

namespace Intellidesk.AcadNet.Common.Core
{
    public class RuleCollection : CollectionBase //, IEnumerable<IRule>
    {
        #region "Property"

        [DefaultValue(null)]
        public static Point3d MainPosition { get; set; }

        [DefaultValue(false)]
        public static bool IncludeNested { get; set; }

        public List<Tuple<string, int?, string>> ConfigDictionary = new List<Tuple<string, int?, string>>();

        public long StartBlockIndex { get; set; }

        public long LayoutId { get; set; }

        public Type[] TypeFilterOn { get; set; }

        public string[] AttributePatternOn { get; set; }

        public string[] LayerPatternOn { get; set; }

        public List<LayerTableItem> LayerItemsFilterOn = new List<LayerTableItem>();

        public string[] LayoutCatalogSite { get; set; }

        public string[] LayoutCatalogOptions { get; set; }

        public string[] TooNameAttributes { get; set; }

        public IRule this[int index]
        {
            get { return ((IRule)List[index]); }
            set { List[index] = value; }
        }

        #endregion

        #region "Methods"

        public int Add(IRule value)
        {
            //var layerItem = new LayerItem { Color = Color.FromColorIndex(ColorMethod.ByAci, value.ColorIndex), Name = value.LayerPlace };
            //if (!LayerManager.ListItems.Contains(layerItem)) LayerManager.ListItems.Add(layerItem);
            return (List.Add(value));
        }

        public void AddRange(IRule[] rules)
        {
            rules.ToList().ForEach(r => Add(r));
        }

        public IRule GetRule(string layerPattern)
        {
            return null;
            //return List.Cast<IRule>().ToList()
            //.Find(r => (r.LayerPatternOn.Contains(layerPattern) && !r.LayerPatternOff.Contains(layerPattern)));
        }

        public int IndexOf(IRule value)
        {
            return (List.IndexOf(value));
        }

        public void Insert(int index, IRule value)
        {
            List.Insert(index, value);
        }

        public void Remove(IRule value)
        {
            List.Remove(value);
        }

        public bool Contains(IRule value)
        {
            // If value is not of type Int16, this will return false.
            return (List.Contains(value));
        }

        protected override void OnInsert(int index, Object value)
        {
            // Insert additional code to be run only when inserting values.
        }

        protected override void OnRemove(int index, Object value)
        {
            // Insert additional code to be run only when removing values.
        }

        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            // Insert additional code to be run only when setting values.
        }

        protected override void OnValidate(object value)
        {
            if (value.GetType().GetInterface("IRule", true) != typeof(IRule))
                throw new ArgumentException("value must be of type Int16.", "value");
        }

        public new IEnumerator<IRule> GetEnumerator()
        {
            for (var i = 0; i < List.Count; i++)
            {
                yield return (IRule)List[i];
            }
        }

        #endregion "Methods"

        #region "Ext.method"

        public Type[] XGetFilterTypesOn()
        {
            var filterTypes = new List<Type>();
            //List.Cast<IRule>().ToList().ForEach(r => filterTypes.AddRange(r.TypeFilterOn));
            return filterTypes.ToArray();
        }

        public string[] XGetFilterAttributesOn()
        {
            var filterTypes = new List<string>();
            List.Cast<IRule>().ToList().ForEach(r => filterTypes.AddRange(r.AttributePatternOn));
            return filterTypes.ToArray();
        }

        public Type[] XGetSingleFilterTypesOn()
        {
            return TypeFilterOn;
        }

        public string[] XGetSingleFilterBlockAttributesOn()
        {
            return AttributePatternOn;
        }

        public List<LayerTableItem> XGetLayerItemsDistinct()
        {
            return null;
            //return List.Cast<IRule>().ToList()
            //    .Select(r => new LayerTableItem
            //    {
            //        Color = Color.FromColorIndex(ColorMethod.ByAci, (short)r.ColorIndex), Name = r.LayerDestination
            //    })
            //.Distinct()
            //.ToList();
        }

        #endregion
    }
}