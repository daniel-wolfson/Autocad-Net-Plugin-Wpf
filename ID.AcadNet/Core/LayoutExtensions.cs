using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using ILayout = Intellidesk.Data.Models.Cad.ILayout;

namespace Intellidesk.AcadNet.Core
{
    /// <summary> Layout extensions methods </summary>
    public static class LayoutExtensions
    {
        /// <summary> defines if layout eadOnly </summary>
        public static bool IsLayoutReadOnly(this ILayout layout)
        {
            string[] logicalDrives = Directory.GetLogicalDrives();

            return logicalDrives.ToList()
                .Any(drive => Application.DocumentManager.Cast<Document>()
                    .Where(doc => doc.IsReadOnly)
                    .Select(doc => doc.Name.ToLower())
                    .Contains((layout.CADFileName.Contains(":") ? layout.CADFileName : drive.Substring(0, 2) + layout.CADFileName).ToLower()));
        }

        /// <summary> defines if layout contained in open documents collection </summary>
        public static bool IsLayoutLoaded(this ILayout layout)
        {
            if (layout == null || String.IsNullOrEmpty(layout.CADFileName)) return false;

            var logicalDrives = Directory.GetLogicalDrives();

            return logicalDrives.ToList()
                .Any(drive => Application.DocumentManager.Cast<Document>()
                    .Select(doc => doc.Name.ToLower())
                    .Contains((layout.CADFileName.Contains(":") ? layout.CADFileName : drive.Substring(0, 2) + layout.CADFileName).ToLower()));
        }

        /// <summary> defines if layout linked MapLoaded </summary>
        public static bool IsLinkedMapLoaded(this ILayout layout)
        {
            if (layout == null || String.IsNullOrEmpty(layout.TABFileName)) return false;

            var logicalDrives = Directory.GetLogicalDrives();

            return logicalDrives.ToList()
                .Any(drive => Application.DocumentManager.Cast<Document>()
                    .Select(doc => doc.Name.ToLower())
                    .Contains((layout.TABFileName.Contains(":") ? layout.TABFileName : drive.Substring(0, 2) + layout.TABFileName).ToLower()));
        }

        /// <summary> defines if layout linked MapLoaded </summary>
        public static bool IsMapLoaded(this ILayout layout)
        {
            if (layout == null || String.IsNullOrEmpty(layout.TABFileName)) return false;

            var logicalDrives = Directory.GetLogicalDrives();

            return logicalDrives.ToList()
                .Any(drive => Application.DocumentManager.Cast<Document>()
                    .Select(doc => doc.Name.ToLower())
                    .Contains((layout.TABFileName.Contains(":") ? layout.TABFileName : drive.Substring(0, 2) + layout.TABFileName).ToLower()));
        }

        /// <summary> FindLayoutFullPath </summary>
        public static string FindLayoutFullPath(this ILayout layout)
        {
            var result = "";
            try
            {
                var logicalDrives = Directory.GetLogicalDrives();
                result = logicalDrives.Select(drive => (layout.CADFileName.Contains(":")
                    ? layout.CADFileName
                    : drive.Substring(0, 2) + layout.CADFileName).ToLower())
                    .FirstOrDefault(File.Exists);
                if (result == null) result = "";
            }
            catch { }
            return result;
        }

        /// <summary> FindMapFullPath </summary>
        public static string FindMapFullPath(this ILayout tab)
        {
            var result = "";
            try
            {
                var logicalDrives = Directory.GetLogicalDrives();
                result = logicalDrives
                    .Select(drive => (tab.TABFileName.Contains(":") ? tab.TABFileName : drive.Substring(0, 2) + tab.TABFileName))
                    .FirstOrDefault(File.Exists);
                if (result == null) result = "";
            }
            catch { }
            return result;
        }

        /// <summary> defines if layout contained in open documents collection </summary>
        public static bool IsPropertiesValid(this ILayout layout)
        {
            return layout != null && layout.IsValid();
        }

        /// <summary> defines if layout contained in open documents collection </summary>
        //public static bool IsPropertiesChanged(this Layout layout)
        //{
        //    return layout != null && layout.ChangedProperties != null && layout.ChangedProperties.Count > 0;
        //}

        /// <summary> Test </summary>
        //public static ObservableCollection<ILayout> GetList(this ObservableCollection<ILayout> query, Func<ILayout, bool> expression)
        //{
        //    return new ObservableCollection<ILayout>(query.Where(expression));
        //}

        /// <summary> Is Lsds Compatible </summary>
        public static object CloneObject(this object objSource)
        {
            //Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);

            //Get all the properties of source object type
            PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to 
                if (property.CanWrite)
                {
                    //check whether property type is value type, enum or string type
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType == typeof(String))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }
                    //else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                    else
                    {
                        object objPropertyValue = property.GetValue(objSource, null);
                        if (objPropertyValue == null)
                        {
                            property.SetValue(objTarget, null, null);
                        }
                        else
                        {
                            property.SetValue(objTarget, objPropertyValue.CloneObject(), null);
                        }
                    }
                }
            }

            return objTarget;
        }

        /// <summary> IsRuleCompatible </summary>
        public static bool IsRuleCompatible(this ILayout layout, IRule rule)
        {
            if (layout == null) return false;

            //var Doc = Application.DocumentManager.MdiActiveDocument;
            //var Db = HostApplicationServices.WorkingDatabase; //Doc.Database;
            //var Ed = Doc.Editor;

            //dynamic bt = Db.BlockTableId;
            //Ed.WriteMessage("\n*** BlockTableRecords in this DWG ***");
            //foreach (dynamic btr in bt)
            //    Ed.WriteMessage("\n" + btr.Name);

            dynamic ms = SymbolUtilityServices.GetBlockModelSpaceId(HostApplicationServices.WorkingDatabase);

            var currentObjectIds =
                ((IEnumerable<dynamic>)ms)
                    .Where(ent => ent.IsKindOf(Type.GetType("Autodesk.AutoCAD.DatabaseServices.Entity.BlockReference")))
                    .Select(x => x.ObjectId)
                    .Cast<ObjectId>()
                    .ToList();

            currentObjectIds.XGetObjects(null, rule.AttributePatternOn);

            return currentObjectIds.Count != 0;

            //return btr.Cast<ObjectId>()
            //        .Select(n => n.GetObject(OpenMode.ForRead) as Entity)
            //        .Where(n => n != null && !(n is AttributeDefinition))
            //        .Max(n => n.GeometricExtents.MaxPoint.Y);

            //var lineStartPoints =
            //  from ent in (IEnumerable<dynamic>)ms
            //  where ent.IsKindOf(typeof(Line))
            //  select ent.StartPoint;

            //foreach (Point3d start in lineStartPoints)
            //    Ed.WriteMessage("\n" + start.ToString());

            //// LINQ query - all entities on layer '0'
            //Ed.WriteMessage("\n\n*** Entities on Layer 0 ***");

            //var entsOnLayer0 =
            //  from ent in (IEnumerable<dynamic>)ms
            //  where ent.Layer == "0"
            //  select ent;

            //foreach (dynamic e in entsOnLayer0)
            //    Ed.WriteMessage("\nHandle=" + e.Handle.ToString() + ", ObjectId=" +
            //        ((ObjectId)e).ToString() + ", Class=" + e.ToString());

            //Ed.WriteMessage("\n\n");
            //// Using LINQ with selection sets

            //PromptSelectionResult res = Ed.GetSelection();
            //if (res.Status != PromptStatus.OK)
            //    return;

            //// Select all entities in selection set that have an object
            //// called "MyDict" in their extension dictionary

            //var extDicts =
            //  from ent in res.Value.GetObjectIds().Cast<dynamic>()
            //  where ent.ExtensionDictionary != ObjectId.Null &&
            //    ent.ExtensionDictionary.Contains("MyDict")
            //  select ent.ExtensionDictionary.Item("MyDict");

            //// Erase our dictionary
            //foreach (dynamic myDict in extDicts)
            //    myDict.Erase();
            //return true;
        }
    }
}