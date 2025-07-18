using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Intellidesk.AcadNet.Common.Extensions
{
    /// <summary> XData - Get, Set, Update XData </summary>
    public static class XDataExtensions
    {
        const string KRegAppName = "Intellidesk";
        const string Ns = "Intellidesk.AcadNet.Common.Model";

        public static IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();
        public static Document Doc => acadApp.DocumentManager.MdiActiveDocument;
        public static Database Db => Doc.Database;

        #region <AddXData>



        /// <summary> AddExtensionDictionary </summary>
        public static ObjectId AddExtensionDictionary(this DBObject obj, OpenMode openMode)
        {
            ObjectId extId;
            using (Transaction tr = obj.Database.TransactionManager.StartTransaction())
            {
                extId = obj.ExtensionDictionary;
                if (extId == ObjectId.Null)
                {
                    obj.UpgradeOpen();
                    obj.CreateExtensionDictionary();
                    extId = obj.ExtensionDictionary;
                }
                tr.Commit();
            }
            return extId;
        }

        /// <summary> add typedValues to XData </summary>
        public static ObjectId XAddXData(this ObjectId objectId, TypedValue[] typedValues)
        {
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                try
                {
                    var rb = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, PluginSettings.Name));
                    typedValues.ToList().ForEach(rb.Add);

                    using (Doc.LockDocument())
                    {
                        var ent = tr.GetObject(objectId, OpenMode.ForWrite);
                        ent.XData = rb;
                    }
                    rb.Dispose();
                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(XDataExtensions)}.{nameof(XAddXData)} error: ", ex);
                }

                tr.Commit();
            }
            return objectId;
        }

        /// <summary> add typedValues to XData </summary>
        public static DBObject XAddXData(this DBObject obj, TypedValue[] typedValues)
        {
            var rb = new ResultBuffer(typedValues);
            obj.XData = rb;
            rb.Dispose();
            return obj;
        }

        /// <summary> add typedValue to XData </summary>
        public static DBObject XAddXData(this DBObject obj, TypedValue typedValue)
        {
            bool existTopTransaction = Doc.Database.TransactionManager.TopTransaction != null;
            Transaction tr = Doc.Database.TransactionManager.TopTransaction ?? Doc.Database.TransactionManager.StartTransaction();

            try
            {
                var rb = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, PluginSettings.Name)) { typedValue };
                obj.XData = rb;
                rb.Dispose();
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.Error($"{nameof(XDataExtensions)}.{nameof(XAddXData)} error: ", ex);
            }
            tr.Commit();

            if (!existTopTransaction)
                tr.Commit();

            return obj;
        }

        /// <summary> add XDataRecord to XData </summary>
        public static DBObject XAddXData(this DBObject obj, XDataRecord dataRec = default(XDataRecord))
        {
            bool existTopTransaction = Doc.Database.TransactionManager.TopTransaction != null;
            Transaction tr = Doc.Database.TransactionManager.TopTransaction ??
                             Doc.Database.TransactionManager.StartTransaction();

            using (Doc.LockDocument())
            {
                try
                {
                    var rb = new ResultBuffer(new TypedValue((int)DxfCode.ExtendedDataRegAppName, PluginSettings.Name))
                    {
                        new TypedValue((int) DxfCode.ExtendedDataAsciiString, dataRec.Name),
                        new TypedValue((int) DxfCode.ExtendedDataHandle, dataRec.Handle),
                        new TypedValue((int) DxfCode.ExtendedDataScale, dataRec.ScaleFactors),
                        new TypedValue((int) DxfCode.ExtendedDataReal, dataRec.Rotation),
                        new TypedValue((int) DxfCode.ExtendedDataReal, dataRec.Position)
                    };

                    obj.UpgradeOpen();
                    obj.XData = rb;
                    rb.Dispose();

                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(XDataExtensions)}.{nameof(XAddXData)} error: ", ex);
                }

            }
            if (!existTopTransaction)
                tr.Commit();
            return obj;
        }

        /// <summary> get Typed Values for ids and  add typedValue to XData</summary>
        public static List<ObjectId> XAddXData(this List<ObjectId> ids, BlockReference br)
        {
            return XAddXData(ids, br.ObjectId);
        }

        /// <summary> get Typed Values for ids and  add typedValue to XData</summary>
        public static List<ObjectId> XAddXData(this List<ObjectId> ids, ObjectId linkObject)
        {
            //var lookup = ids.ToLookup(i => i, cls => cls.ObjectClass.GetRXClass());
            var xdr = (new XDataRecord(linkObject)).GetTypedValues();
            return ids.Select(id => id.XAddXData(xdr)).ToList();
        }

        /// <summary> XAddDataToNod: Add xRecord for Named Object Dictionary to Model Space </summary>
        public static void XAddDataToNod(this Database db, string namedKey, TypedValue[] values)
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            var trans = db.TransactionManager.StartTransaction();
            try
            {
                var nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);
                if (nod.Contains(namedKey))
                {
                    var entryId = nod.GetAt(namedKey);
                    var myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);
                    // Print out the values of the Xrecord to the command line. 
                    foreach (var value in myXrecord.Data)
                    {
                        // Use the WriteMessage method of the editor. 
                        doc.Editor.WriteMessage("\n" + Convert.ToString(value.TypeCode) + " . " + Convert.ToString(value.Value));
                    }
                }
                else
                {
                    nod.UpgradeOpen();
                    var myXrecord = new Xrecord { Data = new ResultBuffer(values) };

                    // Create the entry in the ExtensionDictionary. 
                    nod.SetAt(namedKey, myXrecord);

                    // Tell the transaction about the newly created Xrecord 
                    trans.AddNewlyCreatedDBObject(myXrecord, true);
                }

                // all ok, commit it 
                trans.Commit();
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(XDataExtensions)}.{nameof(XAddDataToNod)} error: ", ex);
            }
            finally
            {
                // whatever happens we must dispose the transaction 
                trans.Dispose();
            }
        }

        public static void XAddDataToNOD(this Database db)
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
            try
            {

                DBDictionary nod = (DBDictionary)trans.GetObject(ed.Document.Database.NamedObjectsDictionaryId, OpenMode.ForRead);
                if (nod.Contains("MyData"))
                {
                    ObjectId entryId = nod.GetAt("MyData");
                    ed.WriteMessage("\n" + "This entity already has data...");

                    Xrecord myXrecord = null;
                    myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);
                    foreach (TypedValue value in myXrecord.Data)
                    {
                        ed.WriteMessage("\n" + value.TypeCode + " . " + value.Value);
                    }
                }
                else
                {
                    nod.UpgradeOpen();

                    Xrecord myXrecord = new Xrecord();

                    ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                        new TypedValue((int)DxfCode.Text, "MyCompanyDefaultSettings"),
                        new TypedValue((int)DxfCode.Real, 51.9),
                        new TypedValue((int)DxfCode.Real, 100.0),
                        new TypedValue((int)DxfCode.Real, 320.6));

                    myXrecord.Data = data;

                    nod.SetAt("MyData", myXrecord);

                    trans.AddNewlyCreatedDBObject(myXrecord, true);
                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                ed.WriteMessage("a problem occurred because " + ex.Message);
            }
            finally
            {
                trans.Dispose();
            }
        }

        public static void XWriteToNOD(this Database db)
        {
            try
            {
                // We will write to C:\Temp\Test.dwg. Make sure it exists!
                // Load it into AutoCAD
                //db.ReadDwgFile(@"C:\Temp\Test.dwg", System.IO.FileShare.ReadWrite, false, null);
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    // Find the NOD in the database
                    DBDictionary nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                    // We use Xrecord class to store data in Dictionaries
                    Xrecord myXrecord = new Xrecord();
                    myXrecord.Data = new ResultBuffer(
                            new TypedValue((int)DxfCode.Int16, 1234),
                            new TypedValue((int)DxfCode.Text,
                                            "This drawing has been processed"));

                    // Create the entry in the Named Object Dictionary
                    nod.SetAt("MyData", myXrecord);
                    trans.AddNewlyCreatedDBObject(myXrecord, true);

                    // Now let's read the data back and print them out
                    //  to the Visual Studio's Output window
                    ObjectId myDataId = nod.GetAt("MyData");
                    Xrecord readBack = (Xrecord)trans.GetObject(
                                                  myDataId, OpenMode.ForRead);
                    foreach (TypedValue value in readBack.Data)
                        Debug.Print("===== OUR DATA: " + value.TypeCode + ". " + value.Value);

                    trans.Commit();

                } // using

                db.SaveAs(@"C:\Temp\Test.dwg", DwgVersion.Current);

            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
            finally
            {
                db.Dispose();
            }
        }

        public static DBObject XAddData(this DBObject obj, IPaletteElement element)
        {
            var dataTypeValues = element.GetDataTypeValues();
            return obj.XAddXData(dataTypeValues);
        }
        #endregion

        #region <GetXDat>

        /// <summary> XGetXData </summary>
        public static Dictionary<string, string> XGetDataDictionary(this DBObject obj)
        {
            var dic = obj.XData.AsArray()
                    .Where(x => x.TypeCode == 1000)
                    .Select(input =>
                    {
                        string[] substrings = Regex.Split(input.Value.ToString(), "_");
                        return new KeyValuePair<string, string>(substrings[0], substrings[1]);
                    })
                    .ToDictionary(x => x.Key, y => y.Value);
            return dic;
        }

        /// <summary> XExistXData </summary>
        public static bool XExistData(this DBObject obj)
        {
            return obj.XData != null && obj.XData.AsArray()
                       .Any(x => x.TypeCode == (short)DxfCode.ExtendedDataRegAppName
                            && x.Value.ToString() == PluginSettings.Name);
        }

        /// <summary> XGetXData </summary>
        public static string XGetData(this DBObject obj, string key)
        {
            if (!obj.XExistData()) return null;

            Dictionary<string, string> dic = obj.XGetDataDictionary();
            return dic.ContainsKey(key) ? obj.XGetDataDictionary()[key] : null;
        }

        /// <summary> XAddXData for ObjectId </summary>
        public static XDataRecord XGetDataRecord(this ObjectId objId)
        {
            var doc = acadApp.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            var xdr = new XDataRecord();
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                if (ent.XData != null)
                {
                    foreach (var tv in ent.XData.AsArray()
                        .Where(x => x.TypeCode == 1001 && x.Value.ToString() == PluginSettings.Name)
                        .Where(x => x.TypeCode != 1001))
                    {
                        var propertyInfos = typeof(XDataRecord).GetProperties(BindingFlags.Public | BindingFlags.Static);
                        foreach (var prop in propertyInfos)
                        {
                            if (tv.Value.ToString().Contains(prop.Name))
                            {
                                //PropertyInfo prop = xdr.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                                var val = Convert.ChangeType(tv.Value, prop.PropertyType);
                                if (prop.CanWrite) { prop.SetValue(xdr, val, null); }
                            }
                        }
                    }
                    //WorkSpace.Ed.SelectAll(New SelectionFilter(New TypedValue() {New TypedValue(0, "Text"), New TypedValue(1, "~*[~.0-9]*"), New TypedValue(1, "~*.*.*")}))
                }
            }
            return xdr;
        }

        /// <summary> XGetXDataObjectType, where typeCode of TypeCodes </summary>
        public static Type XGetXDataObjectType(this DBObject obj, string ownerType = null)
        {
            string dataType = obj.XGetData(ownerType ?? TypeCodes.TypeFullName);
            if (dataType == null) return null;

            Type type;
            string assemblyName;

            if (dataType.Contains("AcadNet.Common"))
                assemblyName = typeof(AcadClosureConnect).Assembly.FullName;
            else
                assemblyName = typeof(PaletteElement).Assembly.FullName;

            type = Type.GetType($"{dataType},{assemblyName}");
            //type = type ?? Type.GetType($"{dataType},{typeof(PaletteElement).Assembly.FullName}");

            return type;
        }

        /// <summary> XGetXDataObjectType </summary>
        public static TElement XGetXDataObject<TElement>(this DBObject obj) where TElement : PaletteElement
        {
            string dataObj = obj.XGetData(TypeCodes.Object);
            if (dataObj != null)
                return JsonConvert.DeserializeObject<TElement>(dataObj);
            return null;
        }

        /// <summary> Get xdata object using JsonConvert.DeserializeObject by type </summary>
        public static IPaletteElement XGetXDataObject(this DBObject obj, Type type)
        {
            if (type == null) return null;

            string dataType = obj.XGetData(TypeCodes.Object);
            if (dataType != null)
                return JsonConvert.DeserializeObject(dataType, type) as IPaletteElement;

            return null;
        }

        public static IPaletteElement XGetDataObject(this DBObject obj)
        {
            try
            {
                Type dataObjectType = obj.XGetXDataObjectType();
                if (dataObjectType == null || !dataObjectType.GetInterfaces().Contains(typeof(IPaletteElement)))
                    return null;

                var dataObject = obj.XGetData(TypeCodes.Object);
                if (dataObject == null) return null;

                return (IPaletteElement)JsonConvert.DeserializeObject(dataObject, dataObjectType);
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error(ex, nameof(XGetXDataObject) + ": {0}");
                return null;
            }
        }

        /// <summary> XGetDictionaryEntryTypedValue </summary>
        public static object XGetDictionaryEntryTypedValue(this ObjectId entryId, int code)
        {
            if (entryId == ObjectId.Null)
                return null;

            object ret = null;
            using (Transaction tr = entryId.Database.TransactionManager.StartTransaction())
            {
                Xrecord entry = (Xrecord)tr.GetObject(entryId, OpenMode.ForRead);
                List<TypedValue> tvList = new List<TypedValue>();
                if (entry.Data == null)
                {
                    ret = null;
                }
                else
                {
                    tvList = entry.Data.AsArray().ToList();
                    int index = tvList.FindIndex(e => e.TypeCode == code);
                    if (index < 0)
                    {
                        ret = null;
                    }
                    else
                    {
                        ret = tvList[index].Value;
                    }
                }

                tr.Commit();
            }
            return ret;
        }

        /// <summary> Retrieve the existing rotation angle from XData </summary>
        public static double GetStoredRotation(this DBObject obj)
        {
            var ret = 0.0;
            var rb = obj.XData;
            if (rb != null)
            {
                // If we find our group code, it means that on
                // the next iteration, we'll get our rotation
                var bReadyForRot = false;
                foreach (TypedValue tv in rb)
                {
                    if (bReadyForRot)
                    {
                        //If tv.TypeCode = kRotCode Then
                        if (tv.Value.ToString().Contains("Rotate="))
                        {
                            ret = Convert.ToDouble(tv.Value.ToString().Split(Convert.ToChar("=")).GetValue(1));
                            //CDbl(tv.Value)
                            bReadyForRot = false;
                        }
                    }
                    if (tv.TypeCode == (int)DxfCode.EmbeddedObjectStart) //KAppCode = 101
                    {
                        bReadyForRot = true;
                    }
                }
                rb.Dispose();
            }
            return ret;
        }

        /// <summary> XGetTypedValues </summary>
        public static TypedValue[] XGetTypedValues(this XDataRecord xdr)
        {
            var typedValues = new List<TypedValue>();
            var type = Type.GetType(xdr.GetType().Name);
            if (type != null)
            {
                var propertyInfos = type.GetProperties(); //typeof(Type).GetProperties(BindingFlags.Public);
                foreach (var prop in propertyInfos)
                {
                    var attributes = TypeDescriptor.GetProperties(xdr)[prop.Name].Attributes;
                    var myAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];
                    if (prop.GetValue(xdr, null) != myAttribute.Value)
                    {
                        typedValues.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString,
                            (prop.Name + ":" + Convert.ToString(prop.GetValue(xdr, null))
                                .Replace("(", "").Replace(")", ""))));
                    }
                }
            }
            return typedValues.ToArray();
        }

        /// <summary> GetObjXData </summary>
        public static object XGetXData(this Document doc, int tCntId)
        {
            return doc.XGetXData(GetSelectOptions.All, tCntId);
        } //GetObjXData

        /// <summary> GetObjXData </summary>
        public static ObjectIdCollection XGetXData(this Document doc, GetSelectOptions tPromptOption = GetSelectOptions.All, int tCntId = -1,
            GetResultOptions tReturnOption = GetResultOptions.ObjectIdColl, string tXDataParameter = "CntId")
        {
            var ed = doc.Editor;
            var n = 0;
            var buff = new StringBuilder("");
            var retObjIds = new ObjectIdCollection();

            // Clear the pickfirst set...
            acadApp.DocumentManager.MdiActiveDocument.Editor.Regen();
            using (var tr = acadApp.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {

                if (tPromptOption == GetSelectOptions.GetObject)
                {
                    var opt = new PromptEntityOptions(Convert.ToChar(10) + "Select construction: ");
                    PromptEntityResult res = ed.GetEntity(opt);
                    if (res.Status != PromptStatus.OK)
                    {
                        return null;
                    }
                    var obj = tr.GetObject(res.ObjectId, OpenMode.ForRead);
                    var rb = obj.XData;
                    if (rb == null)
                    {
                        acadApp.ShowAlertDialog("Entity is not construction" + Convert.ToChar(10));
                        return null;
                        //WorkSpace.Ed.WriteMessage(vbLf & "Entity is not construction")
                    }
                    if (tReturnOption == GetResultOptions.ToDisplay)
                    {
                        foreach (var tv in obj.XData)
                        {
                            buff = buff.AppendLine(String.Format("{0} - {2}", Math.Max(Interlocked.Increment(ref n), n - 1), tv.TypeCode, tv.Value));
                            //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("{0} - {2}" & vbLf, System.Math.Max(System.Threading.Interlocked.Increment(n), n - 1), tv.TypeCode, tv.Value)
                        }
                        rb.Dispose();
                    }
                }

                if (tPromptOption == GetSelectOptions.SelectLast)
                {
                    var psr = ed.SelectLast();
                    if (psr.Status == PromptStatus.OK)
                    {
                        retObjIds = new ObjectIdCollection(psr.Value.GetObjectIds());
                    }
                }
                //Dim tvs As TypedValue() = New TypedValue() {New TypedValue(CInt(DxfCode.[Operator]), "<or"), _
                //                          New TypedValue(CInt(DxfCode.Start), "TEXT, LWPOLYLINE, LINE"), _
                //                          New TypedValue(CInt(DxfCode.[Operator]), "<and"), _
                //                          New TypedValue(CInt(DxfCode.ExtendedDataAsciiString), "CntId=" + tCntId.ToString.Trim), _
                //                          New TypedValue(CInt(DxfCode.[Operator]), "and>"), _
                //                          New TypedValue(CInt(DxfCode.[Operator]), "or>")}
                //Dim sf As New SelectionFilter(tvs)
                //Dim psr As PromptSelectionResult = WorkSpace.Ed.SelectAll(sf)
                //If psr.Status = PromptStatus.OK Then
                //RetObjIds = New ObjectIdCollection(psr.Value.GetObjectIds())

                if (tReturnOption == GetResultOptions.ObjectIdColl | tReturnOption == GetResultOptions.ToDisplay)
                {
                    var bt = (BlockTable)tr.GetObject(acadApp.DocumentManager.MdiActiveDocument.Database.BlockTableId, OpenMode.ForRead);
                    var btr = (BlockTableRecord)tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForRead);

                    foreach (ObjectId objId in btr)
                    {
                        var ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                        if (ent.XData != null)
                        {
                            foreach (TypedValue tv in ent.XData)
                            {
                                if (tReturnOption == GetResultOptions.ToDisplay)
                                {
                                    buff = buff.AppendLine(String.Format("{0} - {2}", Math.Max(Interlocked.Increment(ref n), n - 1), tv.TypeCode, tv.Value));
                                }
                                else
                                {
                                    if (tv.TypeCode == 1000 & tv.Value.ToString().Contains("CntId=" + (tCntId < 0 ? "" : Convert.ToString(tCntId))))
                                    {
                                        retObjIds.Add(ent.ObjectId);
                                    }
                                }
                            }
                            //WorkSpace.Ed.SelectAll(New SelectionFilter(New TypedValue() {New TypedValue(0, "Text"), New TypedValue(1, "~*[~.0-9]*"), New TypedValue(1, "~*.*.*")}))
                            buff = buff.AppendLine(Convert.ToString(Convert.ToChar(10)));
                            n = 0;
                        }
                    }
                }

                // Clear the pickfirst set...
                //WorkSpace.Ed.SetImpliedSelection(New ObjectId(-1) {})

                // ...but highlight the objects
                if (retObjIds.Count > 0)
                {
                    //CmdManager.HighlightEntities(RetObjIds)
                }

                if (tReturnOption == GetResultOptions.ToDisplay)
                {
                    acadApp.ShowAlertDialog(buff.ToString());
                }
            }
            return retObjIds;
        }

        /// <summary> XDataHasElement </summary>
        public static bool XHasXDataElement(this DBObject dbObject)
        {
            if (dbObject.XData == null) return false;

            return dbObject.XData.AsArray()
                .Any(x =>
                     x.TypeCode == (short)DxfCode.ExtendedDataRegAppName &&
                     x.Value.ToString() != PluginSettings.Name);
        }

        #endregion

        #region <XSetData>

        /// <summary> UpdateXData </summary>
        public static void XUpdateXData(this Document doc, ObjectId tObjId, TypeCode typeCode, int value)
        {
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                using (doc.LockDocument())
                {
                    //if (tObjId == ObjectId.Null)
                    //{
                    //    var opt = new PromptEntityOptions("\nSelect construction: "); //Constants.vbLf
                    //    PromptEntityResult res = ed.GetEntity(opt);
                    //    if (res.Status != PromptStatus.OK)
                    //    {
                    //        return;
                    //    }
                    //    obj = (Entity)Tr.GetObject(res.ObjectId, OpenMode.ForWrite);
                    //}
                    //else
                    //{
                    //    obj = (Entity)Tr.GetObject(tObjId, OpenMode.ForWrite);
                    //}

                    var obj = (Entity)tr.GetObject(tObjId, OpenMode.ForWrite);
                    var rb = new ResultBuffer(new TypedValue((int)XDataTypeCode.App, PluginSettings.Name))
                    {
                        new TypedValue((int) typeCode, value)
                    };
                    obj.XData = rb;
                    rb.Dispose();
                }
            }
        } // Update XData

        /// <summary> GetObjXData </summary>
        public static void XSetDataByObjectId(this Document doc, ObjectId tObjId, params string[] tParams)
        {
            var ed = doc.Editor;
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                using (doc.LockDocument())
                {
                    Entity obj;
                    if (tObjId == ObjectId.Null)
                    {
                        var opt = new PromptEntityOptions("\nSelect construction: "); //Convert.ToChar(10) +
                        PromptEntityResult res = ed.GetEntity(opt);
                        if (res.Status != PromptStatus.OK)
                        {
                            return;
                        }
                        obj = (Entity)tr.GetObject(res.ObjectId, OpenMode.ForWrite);
                    }
                    else
                    {
                        obj = (Entity)tr.GetObject(tObjId, OpenMode.ForWrite);
                    }
                    if (obj.XData == null)
                    {
                        doc.AddRegAppTableRecord(PluginSettings.Name);
                        var rb = new ResultBuffer(new TypedValue((int)XDataTypeCode.App, PluginSettings.Name))
                        {
                            new TypedValue((int)XDataTypeCode.Data, obj.GetType().ToString())
                        };
                        //', New TypedValue(1000, tParamValue(0))
                        foreach (var p in tParams)
                        {
                            rb.Add(new TypedValue((int)XDataTypeCode.Data, p));
                            //'rb.Add(New TypedValue(DxfCode.ExtendedDataRegAppName, p))
                        }
                        //'Try
                        //'    rb.Add(New TypedValue(DxfCode.ExtendedDataInteger16, 9999))
                        //'    rb.Add(New TypedValue(DxfCode.ExtendedDataInteger32, 999999))
                        //'    rb.Add(New TypedValue(DxfCode.ExtendedDataReal, 3600.0))
                        obj.XData = rb;
                        //'Catch e As Exception
                        //'    WorkSpace.Ed.WriteMessage(e.Message)
                        //'End Try
                        rb.Dispose();
                    }
                    else
                    {
                        foreach (var p in tParams)
                        {
                            var xdExist = false;
                            foreach (TypedValue x in obj.XData)
                            {
                                if (!x.Value.ToString().Contains(p))
                                {
                                    xdExist = true;
                                }
                            }
                            if (xdExist)
                                obj.XData.Add(new TypedValue((int)XDataTypeCode.Data, p));
                        }
                    }
                    tr.Commit();
                }
            }
        } // Set XData

        /// <summary> Store our rotation angle as XData </summary>
        public static void SetStoredRotation(this DBObject obj, double rotation)
        {
            var rb = obj.XData;
            if (rb == null)
            {
                rb = new ResultBuffer(
                    new TypedValue((int)DxfCode.EmbeddedObjectStart, KRegAppName), //KAppCode = 101
                    new TypedValue((int)DxfCode.ExtendedDataReal, rotation)); //KRotCode = 1040
            }
            else
            {
                // We can simply add our values - no need to remove the previous ones, the new ones are the ones that get stored
                rb = new ResultBuffer();
                foreach (var typedVal in obj.XData)
                {
                    rb.Add(typedVal.Value.ToString().Contains("Rotate=")
                        ? new TypedValue(typedVal.TypeCode, "Rotate=" + rotation)
                        : new TypedValue(typedVal.TypeCode, typedVal.Value));
                }
            }
            obj.XData = rb;
            rb.Dispose();
        } // SetStoredRotation

        /// <summary> XUpdateDictionaryEntryTypedValue </summary>
        public static void XUpdateDictionaryEntryTypedValue(this ObjectId entryId, int code, double value)
        {
            using (Transaction tr = entryId.Database.TransactionManager.StartOpenCloseTransaction())
            {
                Xrecord entry = (Xrecord)tr.GetObject(entryId, OpenMode.ForWrite);
                List<TypedValue> tvList = new List<TypedValue>();
                if (entry.Data == null)
                {
                    tvList.Add(new TypedValue(code, value));
                }
                else
                {
                    tvList = entry.Data.AsArray().ToList();
                    int index = tvList.FindIndex(e => e.TypeCode == code);
                    if (index < 0)
                    {
                        tvList.Add(new TypedValue(code, value));
                    }
                    else
                    {
                        tvList[index] = new TypedValue(code, value);
                    }
                }
                entry.Data = new ResultBuffer(tvList.ToArray());

                tr.Commit();
            }
        }

        public static TElement XUpdateDataObject<TElement>(this DBObject obj, Func<PaletteElement, PaletteElement> fn) where TElement : PaletteElement
        {
            TElement element = obj.XGetXDataObject<TElement>();
            element = fn(element) as TElement;
            obj.XAddData(element);
            return element;
        }
        public static void XUpdateDataObject(this DBObject obj, IEnumerable<ObjectIdItem> items)
        {
            var element = obj.XGetDataObject();
            element.Items = element.Items.Concat(items.Select(x => x.ObjectId.Handle.ToString())).ToArray();
            obj.XAddData(element);
        }

        public static void XOpenForWrite(this ObjectId id, IEnumerable<ObjectIdItem> items)
        {
            id.XOpenForWrite(ent =>
            {
                var el = ent.XGetDataObject();
                var concatItems = items.Select(x => x.ObjectId.Handle.ToString()).ToArray();
                el.Items = concatItems;
                ent.XAddData(el);
            });
        }

        public static void XSetProjectInfo(this Database db, List<TypedValue> layoutInfoList)
        {
            IPluginSettings appSettings = Plugin.GetService<IPluginSettings>();
            Document doc = acadApp.DocumentManager.GetDocument(db);
            try
            {
                using (doc.LockDocument())
                {
                    using (Transaction trans = db.TransactionManager.StartTransaction())
                    {
                        // Find the NOD in the database
                        DBDictionary nod =
                            (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                        if (!nod.Contains(appSettings.Name))
                        {

                            // We use Xrecord class to store data in Dictionaries
                            Xrecord myXrecord = new Xrecord();
                            var data = new ResultBuffer();

                            if (layoutInfoList.Count > 0)
                                foreach (var info in layoutInfoList)
                                    data.Add(info);

                            myXrecord.Data = data;
                            //new TypedValue((int)DxfCode.Int16, ProjectTypedValues.CoordSystem),
                            //new TypedValue((int)DxfCode.Text, "ITM"));

                            //var extents = Geoms.Extents();
                            //myXrecord.Data.Add(new TypedValue((int)DxfCode.ExtendedDataWorldXCoordinate,
                            //    extents.MinPoint));
                            //myXrecord.Data.Add(new TypedValue((int)DxfCode.ExtendedDataWorldYCoordinate,
                            //    extents.MaxPoint));



                            // Create the entry in the Named Object Dictionary
                            nod.SetAt(appSettings.Name, myXrecord);
                            trans.AddNewlyCreatedDBObject(myXrecord, true);

                            trans.Commit();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
            finally
            {
                db.Dispose();
            }
        }

        #endregion

        /// <summary> RemExtDict </summary>
        public static void RemoveExtDict()
        {
            PromptEntityOptions opt = new PromptEntityOptions("\n:") { AllowNone = true };

            PromptEntityResult res = Doc.Editor.GetEntity(opt);
            if (res.Status != PromptStatus.OK) return;

            try
            {
                using (Entity en = res.ObjectId.GetObject(OpenMode.ForWrite) as Entity)
                {
                    ObjectId idExt = en.ExtensionDictionary;
                    if (idExt.IsNull || idExt.IsErased)
                    {
                        Doc.Editor.WriteMessage("\n!");
                        return;
                    }

                    using (DBDictionary dict = idExt.GetObject(OpenMode.ForWrite) as DBDictionary)
                    {
                        List<string> keys = new List<string>();
                        using (DbDictionaryEnumerator dictEnum = dict.GetEnumerator())
                        {
                            while (dictEnum.MoveNext())
                                keys.Add(dictEnum.Key);
                        }

                        foreach (string key in keys) dict.Remove(key);
                    }

                    en.ReleaseExtensionDictionary();
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Doc.Editor.WriteMessage("\n: {0}",
                  ex.ToString());
            }
        }

    }
}