using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> XRecord extensions </summary>
    public static class XRecordExtensions
    {
        public static IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();
        public static Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        public static Database Db => HostApplicationServices.WorkingDatabase;

        private static ObjectId _appNamedObjectsDictionaryId;
        public static ObjectId AppNamedObjectsDictionaryId => _appNamedObjectsDictionaryId;

        static XRecordExtensions()
        {
            _appNamedObjectsDictionaryId = XCreateDictionaryNOD();
        }

        #region <Get>

        /// <summary> XGetXrecord by ExtensionDictionary </summary>
        public static Dictionary<DxfCodeExt, object> XGetXrecord(this DBObject obj)
        {
            var result = new Dictionary<DxfCodeExt, object>();
            using (var tr = obj.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary xDict = (DBDictionary)tr.GetObject(AppNamedObjectsDictionaryId, OpenMode.ForRead, false);
                    if (xDict.Count > 0)
                    {
                        var xRec = (Xrecord)tr.GetObject(xDict.GetAt(obj.Handle.ToString()), OpenMode.ForRead, false);
                        if (xRec.Data != null)
                            result = xRec.Data.AsArray().ToDictionary(key => (DxfCodeExt)key.TypeCode, val => val.Value);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
                    result = null;
                }
                return result;
            }
        }

        /// <summary> XGetExtDataRecord </summary>
        public static List<XDataRecord> XGetXRecordObject(this DBObject obj)
        {
            var xDataRecordsList = new List<XDataRecord>();
            using (var tr = obj.Database.TransactionManager.StartTransaction())
                try
                {
                    // Declare an Entity variable named ent.  
                    var ent = (Entity)tr.GetObject(obj.ObjectId, OpenMode.ForRead);
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        // Upgrade the open of the entity. 
                        ent.UpgradeOpen();
                        ent.CreateExtensionDictionary();
                    }

                    var extensionDict = (DBDictionary)tr.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);
                    if (extensionDict.Contains(PluginSettings.Name))
                    {
                        // Check to see if the entry we are going to add is already there. 
                        var entryId = extensionDict.GetAt(PluginSettings.Name);
                        var myXrecord = (Xrecord)tr.GetObject(entryId, OpenMode.ForRead);

                        foreach (var xrecord in myXrecord.Data)
                        {
                            var dict = new XDataRecord { Key = xrecord.Value.ToString() };
                            xDataRecordsList.Add(dict);
                        }
                    }
                    return xDataRecordsList;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(XRecordExtensions)}.{nameof(XGetXRecordObject)} error: ", ex);
                }
            return null;
        }

        public static Type XGetXrecordObjectType(this DBObject obj)
        {
            Type type = null;
            var xRecord = obj.XGetXrecord();
            if (xRecord != null && xRecord.Any())
            {
                var xRecordDataType = obj.XGetXrecord()[DxfCodeExt.DataType].ToString();
                type = Type.GetType($"Intellidesk.AcadNet.Common.Model.{xRecordDataType}, ID.AcadNet.Model", false, true);
            }
            return type;
        }

        /// <summary> GetXrecord </summary>
        public static TElement XGetXrecordObject<TElement>(this DBObject obj) where TElement : PaletteElement
        {
            var xRecordData = obj.XGetXrecord()[DxfCodeExt.DataObject].ToString();
            TElement element = JsonConvert.DeserializeObject<TElement>(xRecordData);
            return element;
        }

        /// <summary> GetXrecord </summary>
        public static Intellidesk.Data.Models.Entities.PaletteElement XGetXrecordObject(this DBObject obj)
        {
            var xRecordType = obj.XGetXrecord()[DxfCodeExt.DataType].ToString();
            Type type = Type.GetType($"Intellidesk.AcadNet.Common.Model.{xRecordType},{typeof(AcadCable).Assembly.FullName}");
            var xRecordData = obj.XGetXrecord()[DxfCodeExt.DataObject].ToString();

            Intellidesk.Data.Models.Entities.PaletteElement element = null;
            element = (Intellidesk.Data.Models.Entities.PaletteElement)JsonConvert.DeserializeObject(xRecordData, type);
            //if (type == typeof(AcadCable))
            //    element = JsonConvert.DeserializeObject<AcadCable>(xRecordData);
            //else if (type == typeof(AcadClosure))
            //    element = JsonConvert.DeserializeObject<AcadClosure>(xRecordData);
            //else if (type == typeof(AcadTitle))
            //    element = JsonConvert.DeserializeObject<AcadTitle>(xRecordData);
            return element;
        }

        /// <summary> GetXrecord </summary>
        public static object XGetXrecordValue(this DBObject obj, DxfCodeExt typeCode)
        {
            var xRecord = obj.XGetXrecord();
            if (xRecord != null && xRecord.Count > 0)
                return obj.XGetXrecord()[typeCode];
            return null;
        }

        /// <summary> GetXrecord </summary>
        public static List<DBObject> XGetXrecordObjects(this DBObject obj)
        {
            List<DBObject> result = new List<DBObject>();
            using (var tr = obj.Database.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(obj.ObjectId, OpenMode.ForRead, false) as Entity;
                if (ent != null)
                {
                    try
                    {
                        DBDictionary xDict =
                            (DBDictionary)
                                tr.GetObject(Doc.Database.NamedObjectsDictionaryId, OpenMode.ForRead, false);
                        ICollection xKeys = ((IDictionary)xDict).Keys;

                        foreach (string xKey in xKeys)
                        {
                            Entity entityItem =
                                tr.GetObject(obj.Database.XGetObjectId(xKey), OpenMode.ForRead, false) as Entity;
                            result.Add(entityItem);
                        }
                    }
                    catch
                    {
                        return result;
                    }
                }
            }
            return result;
        }

        /// <summary> Gets an xrecord data in a named dictionary </summary>
        /// <returns>The xrecord data or null if the dictionary or the xrecord do not exist</returns>
        public static ResultBuffer XGetXrecordNOD(string dictName, string key)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary NOD = (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                if (!NOD.Contains(dictName))
                    return null;

                DBDictionary dict = tr.GetObject(NOD.GetAt(dictName), OpenMode.ForRead) as DBDictionary;
                if (dict == null || !dict.Contains(key))
                    return null;

                Xrecord xRec = tr.GetObject(dict.GetAt(key), OpenMode.ForRead) as Xrecord;

                return xRec?.Data;
            }
        }

        #endregion

        #region <Add>

        public static ObjectId XCreateDictionaryNOD(string dictName = null)
        {
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                dictName = dictName ?? PluginSettings.Name;
                DBDictionary nod = (DBDictionary)tr.GetObject(Db.NamedObjectsDictionaryId, OpenMode.ForRead);

                if (nod.Contains(dictName))
                    return ((DBDictionary)nod[dictName]).ObjectId;

                nod.UpgradeOpen();
                return nod.SetAt(dictName, new DBDictionary());
            }
        }

        public static void XDynamicCreateDictionaryNOD(string dictName, string xRecName, string value)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            dynamic nod = db.NamedObjectsDictionaryId;
            dynamic newdict = nod.SetAt(dictName, new DBDictionary());
            dynamic record = newdict.SetAt(xRecName, new Xrecord());
            ResultBuffer rb = new ResultBuffer();
            rb.Add(new TypedValue((int)DxfCode.Text, value));
            record.Data = rb;
        }

        public static void XDynamicXGetXRecord(string dictName, string xRecName)
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            dynamic nod = db.NamedObjectsDictionaryId;
            try
            {
                var xr = nod[dictName][xRecName];
                ed.WriteMessage("\ndfgdfg" + xr.Data.ToString());
            }
            catch (Exception acadEx)
            {
                if (acadEx.ErrorStatus == ErrorStatus.KeyNotFound)
                    ed.WriteMessage("\nDictionary or record does not exist");
            }
        }

        /// <summary> SetXrecord </summary>
        public static DBObject XAddXrecord(this DBObject obj, ObjectId extDictionaryId, string xRecordKey, params TypedValue[] values)
        {
            using (var tr = obj.Database.TransactionManager.StartTransaction())
            {
                if (extDictionaryId.IsNull)
                {
                    if (obj.ExtensionDictionary.IsNull)
                    {
                        obj.UpgradeOpen();
                        obj.CreateExtensionDictionary();
                    }
                    extDictionaryId = obj.ExtensionDictionary;
                }

                DBDictionary xdict = (DBDictionary)tr.GetObject(extDictionaryId, OpenMode.ForRead);
                Xrecord xrec;
                if (xdict.Contains(xRecordKey))
                {
                    xrec = (Xrecord)tr.GetObject((ObjectId)xdict[xRecordKey], OpenMode.ForRead);
                    xrec.UpgradeOpen();
                    xrec.Data = new ResultBuffer(values);
                }
                else
                {
                    xdict.UpgradeOpen();
                    xrec = new Xrecord { Data = new ResultBuffer(values) };

                    xdict.SetAt(xRecordKey, xrec);
                    tr.AddNewlyCreatedDBObject(xrec, true);

                    //obj.XAddXData(new XDataRecord(xdict.ObjectId));
                }
            }
            return obj;
        }

        /// <summary> XAddXrecord </summary>
        public static DBObject XAddXrecord(this DBObject obj, TypedValue[] typedValues)
        {
            using (var tr = obj.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    if (obj.ExtensionDictionary.IsNull)
                    {
                        obj.UpgradeOpen();
                        obj.CreateExtensionDictionary();
                    }

                    var extensionDict = (DBDictionary)tr.GetObject(obj.ExtensionDictionary, OpenMode.ForRead);
                    if (extensionDict.Contains(PluginSettings.Name))
                    {
                        // Check to see if the entry we are going to add is already there. 
                        var dictId = extensionDict.GetAt(PluginSettings.Name);
                        var myXrecord = (Xrecord)tr.GetObject(dictId, OpenMode.ForRead);
                        myXrecord.UpgradeOpen();
                        myXrecord.Data = new ResultBuffer(typedValues);
                    }
                    else
                    {
                        extensionDict.UpgradeOpen();

                        //  Create a new XRecord. 
                        var myXrecord = new Xrecord();
                        var rb = new ResultBuffer(
                            new TypedValue((int)DxfCode.ExtendedDataRegAppName, PluginSettings.Name))
                            {
                                typedValues.Select(x => new TypedValue(x.TypeCode, x.Value))
                            };

                        // Add the ResultBuffer to the Xrecord 
                        myXrecord.Data = rb;

                        // Create the entry in the ExtensionDictionary. 
                        extensionDict.SetAt(PluginSettings.Name, myXrecord);

                        // Tell the transaction about the newly created Xrecord 
                        tr.AddNewlyCreatedDBObject(myXrecord, true);
                        // Here we will populate the treeview control with the new data 
                        //if (myPalette != null)
                        //{
                        //    foreach (System.Windows.Forms.TreeNode node in myPalette.treeView1.Nodes)
                        //    {

                        //        // Test to see if the node Tag is the ObjectId 
                        //        // of the ent 
                        //        if (node.Tag.ToString() == ent.ObjectId.ToString())
                        //        {

                        //            // Now add the new data to the treenode. 
                        //            System.Windows.Forms.TreeNode childNode = node.Nodes.Add("Extension Dictionary");

                        //            // Add the data. 
                        //            foreach (TypedValue value in myXrecord.Data)
                        //            {
                        //                // Add the value from the TypedValue 
                        //                childNode.Nodes.Add(value.ToString());
                        //            }

                        //            // Exit the for loop (all done - break out of the loop) 
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    return obj;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(XRecordExtensions)}.{nameof(XAddXrecord)} error: ", ex);
                }
            }
            return null;
        }

        public static void XAddXRecodrdData(this Database db)
        {
            // get the editor object 
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            // pick entity to add data to! 
            PromptEntityResult getEntityResult = ed.GetEntity("Pick an entity to add an Extension Dictionary to : ");
            // if all was ok 
            if ((getEntityResult.Status == PromptStatus.OK))
            {
                // now start a transaction 
                Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
                try
                {
                    Entity ent = (Entity)trans.GetObject(getEntityResult.ObjectId, OpenMode.ForRead);
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        ent.UpgradeOpen();
                        ent.CreateExtensionDictionary();
                    }

                    DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                    if (extensionDict.Contains("MyData"))
                    {
                        ObjectId entryId = extensionDict.GetAt("MyData");
                        ed.WriteMessage("\nThis entity already has data...");
                        Xrecord myXrecord = default(Xrecord);
                        myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                        foreach (TypedValue value in myXrecord.Data)
                        {

                            // 12. Use the WriteMessage method of the Editor created above. (ed). 
                            // for the string argument use something like this: 
                            // "\n" + value.TypeCode.ToString() + " . " + value.Value.ToString() 
                            ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());

                        }
                    }
                    else
                    {
                        // 13. If the code gets to here then the data entry does not exist 
                        // upgrade the ExtensionDictionary created in step 5 to write by calling 
                        // the UpgradeOpen() method 
                        extensionDict.UpgradeOpen();

                        System.Type t = typeof(DBDictionary);


                        // 14. Create a new XRecord. Declare an Xrecord variable as a New Xrecord 
                        Xrecord myXrecord = new Xrecord();

                        // 15. Create the resbuf list. Declare a ResultBuffer variable. Instantiate it 
                        // by creating a New ResultBuffer. For the ParamArray of TypeValue for the new 
                        // ResultBuffer use the following: 
                        //new TypedValue((int)DxfCode.Int16, 1),
                        //new TypedValue((int)DxfCode.Text, "MyStockData"),
                        //new TypedValue((int)DxfCode.Real, 51.9),
                        //new TypedValue((int)DxfCode.Real, 100.0),
                        //new TypedValue((int)DxfCode.Real, 320.6)
                        ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                            new TypedValue((int)DxfCode.Text, "MyStockData"),
                            new TypedValue((int)DxfCode.Real, 51.9),
                            new TypedValue((int)DxfCode.Real, 100.0),
                            new TypedValue((int)DxfCode.Real, 320.6));

                        myXrecord.Data = data;

                        extensionDict.SetAt("MyData", myXrecord);

                        trans.AddNewlyCreatedDBObject(myXrecord, true);
                        foreach (TypedValue value in myXrecord.Data)
                        {
                            //childNode.Nodes.Add(value.ToString());
                        }
                    }

                    // all ok, commit it 

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    // a problem occured, lets print it 
                    ed.WriteMessage("a problem occured because " + ex.Message);
                }
                finally
                {
                    // whatever happens we must dispose the transaction 

                    trans.Dispose();

                }

            }
        }

        #endregion

        #region <Set>

        /// <summary> Add or edit a Xrecord data in a named dictionary (the dictionary and xrecord are created if not already exist) </summary>
        /// <param name="dictName">The dictionary name</param>
        /// <param name="key">the xrecord key</param>
        /// <param name="resbuf">the xrecord data</param>
        public static void SetXrecordNOD(string dictName, string key, ResultBuffer resbuf)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary NOD =
                    (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                DBDictionary dict;
                if (NOD.Contains(dictName))
                {
                    dict = (DBDictionary)tr.GetObject(NOD.GetAt(dictName), OpenMode.ForWrite);
                }
                else
                {
                    dict = new DBDictionary();
                    NOD.UpgradeOpen();
                    NOD.SetAt(dictName, dict);
                    tr.AddNewlyCreatedDBObject(dict, true);
                }
                Xrecord xRec = new Xrecord();
                xRec.Data = resbuf;
                dict.SetAt(key, xRec);
                tr.AddNewlyCreatedDBObject(xRec, true);
                tr.Commit();
            }
        }

        /// <summary> SetXrecord </summary>
        public static DBObject XSetXrecord(this DBObject obj, Intellidesk.Data.Models.Entities.PaletteElement element)
        {
            return obj.XSetXrecord(AppNamedObjectsDictionaryId, element);
        }

        /// <summary> SetXrecord </summary>
        public static DBObject XSetXrecord(this DBObject obj, ObjectId extDictionaryId, Intellidesk.Data.Models.Entities.PaletteElement element)
        {
            return obj.XSetXrecord(extDictionaryId, obj.Handle.ToString(), element);
        }

        /// <summary> SetXrecord </summary>
        public static DBObject XSetXrecord(this DBObject obj, ObjectId extDictionaryId, string xRecordKey, Intellidesk.Data.Models.Entities.PaletteElement element)
        {
            element.Handle = obj.Handle.ToString();
            obj.XAddXrecord(extDictionaryId, xRecordKey, element.GetDataTypeValues());
            return obj;
        }

        public static List<DBObject> XGetXrecords(ObjectId extDictionaryId)
        {
            var db = acadApp.DocumentManager.MdiActiveDocument.Database;
            extDictionaryId = extDictionaryId == ObjectId.Null ? db.NamedObjectsDictionaryId : extDictionaryId;
            List<DBObject> resultList = new List<DBObject>();

            using (var tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary xrecords = (DBDictionary)tr.GetObject(extDictionaryId, OpenMode.ForRead);
                foreach (DBDictionaryEntry entry in xrecords)
                {
                    DBObject dbObject = tr.GetObject(entry.Value, OpenMode.ForRead);
                    resultList.Add(dbObject);
                }
            }

            return resultList;
        }

        #endregion
    }
}