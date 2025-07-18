using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;

using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.Infrastructure;
using Microsoft.Practices.Unity;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace Intellidesk.AcadNet.Services
{
    public static class EditServiceExt
    {
        public static IOperationService _operationService;

        //Extension Method: converting to Dictionary for view collection of List<XDataRecord>
        public static Dictionary<Handle, XDataRecord> XToDictionary(this List<XDataRecord> tList)
        {
            return tList.ToDictionary(p => p.Handle);
        }
        public static Entity XGetObject(this Handle hnd)
        {
            ObjectId id;
            if (_operationService.Db.TryGetObjectId(hnd, out id))
            {
                return (Entity)id.GetObject(OpenMode.ForRead);
            }
            return null;
        }
    }

    //Manager for editing of objects in Model Space

    public class EditService :  IEditService
    {
        private readonly IUnityContainer _unityContainer;
        private readonly IPluginManager _pluginManager;
        private IOperationService _operationService;

        //private IOperationService _operationService;
        //private IOperationService OperationService
        //{
        //    get
        //    {
        //        if (_operationService == null)
        //        {
        //            _operationService = _unityContainer.Resolve<OperationService>();
        //            AddRegAppTableRecord(ProjectManager.Name);
        //        }
        //        return _operationService;
        //    }
        //}

        public EditService(IUnityContainer unityContainer, IOperationService operationService, IPluginManager pluginManager)
        {
            _unityContainer = unityContainer;
            _operationService = operationService;
            _pluginManager = pluginManager;
            EditServiceExt._operationService = _operationService;
        }

        //T XToDictionary1<T1, T>(this IEnumerable<T> tList, object obj)
        //{
        //    return (Dictionary<T1, T>)tList.ToDictionary(p => p.ToString());
        //} 

        public double RotateCnt(int tCntId = -1, OptionsGetSelect tOption = OptionsGetSelect.SelectLast)
        {
            const double functionReturnValue = 0;
            double newAngle = 0;
            var objId = ObjectId.Null;

            using (var tr = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {
                while (objId == ObjectId.Null)
                {
                    if (tOption == OptionsGetSelect.GetObject | tCntId < 0)
                    {
                        // First we prompt for the entity to rotate
                        var selectService = _unityContainer.Resolve<SelectService>();
                        objId = selectService.GetEntities(OptionsGetSelect.GetObject, "Select object to rotate").First().ObjectId;
                        if (objId == ObjectId.Null)
                            return functionReturnValue;
                    }
                    else
                    {
                        objId = GetObjXData(OptionsGetSelect.All, tCntId)[0];
                        if (objId == ObjectId.Null)
                            tCntId = -2;
                    }
                }

                var obj = tr.GetObject(objId, OpenMode.ForRead);
                var ent = (Entity)obj;
                var rotationPoint = Point3d.Origin;
                // Use the origin as the default center
                var pl = obj as Polyline;
                if (pl != null)
                {
                    var ps0 = pl.GetLineSegmentAt(0);
                    var ps1 = pl.GetLineSegmentAt(1);
                    var vec = (ps1.EndPoint - ps1.StartPoint);
                    if (tOption == OptionsGetSelect.GetPoint)
                    {
                        var peo = new PromptPointOptions("")
                                      {
                                          Message =
                                              Convert.ToChar(10).ToString(CultureInfo.InvariantCulture) +
                                              "Select point to rotate: "
                                      };
                        var per = Application.DocumentManager.MdiActiveDocument.Editor.GetPoint(peo);
                        if (per.Status == PromptStatus.OK)
                        {
                            rotationPoint = per.Value;
                        }
                        else
                        {
                            rotationPoint = pl.StartPoint + vec;
                        }
                    }
                    else
                    {
                        rotationPoint = pl.StartPoint + vec;
                    }
                }

                var lin = obj as Line;
                if (lin != null)
                {
                    //var lineSegment = new LineSegment3d(lin.StartPoint, lin.EndPoint);
                    //var vec = (lin.EndPoint - lin.StartPoint) / 2.0;
                    //lin.EndPoint.GetAsVector
                    rotationPoint = lin.StartPoint;
                    //+ vec
                }

                var txt = obj as DBText;
                if (txt != null)
                {
                    rotationPoint = txt.Position;
                }

                // Get the base rotation angle stored with the entity, if there was one (default is 0.0)
                var baseAngle = GetStoredRotation(obj);
                //EditInputManagement.GetObjXData(obj)
                newAngle = baseAngle;

                if (ent != null)
                {
                    var ucs = _operationService.Ed.CurrentUserCoordinateSystem;
                    //' Get the current UCS, to pass to the Jig
                    var jig = new RotateJig(ent, rotationPoint, baseAngle, ucs);
                    //' Create our jig object
                    var res = _operationService.Ed.Drag(jig);
                    //AddHandler WorkSpace.Ed.Dragging, AddressOf Drg1
                    //Dim ppr As PromptPointResult = WorkSpace.Ed.Drag(WorkSpace.Ed.SelectLast.Value, vbLf & "Select text location: ", _
                    //                                      Function(pt As Point3d, ByRef mat As Matrix3d)
                    //                                          ' If no change has been made, say so
                    //                                          If mtLoc = pt Then
                    //                                              Return SamplerStatus.NoChange
                    //                                          Else
                    //                                              ' Otherwise we return the displacement
                    //                                              ' matrix for the current position
                    //                                              mat = Matrix3d.Displacement(mtLoc.GetVectorTo(pt))
                    //                                          End If
                    //                                          Return SamplerStatus.OK
                    //                                      End Function)
                    if (res.Status == PromptStatus.OK)
                    {
                        newAngle = jig.GetRotation();
                        //' Get the overall rotation angle and dispose of the temp clone
                        jig.GetEntity().Dispose();
                        var trans = Matrix3d.Rotation(newAngle - baseAngle, ucs.CoordinateSystem3d.Zaxis, rotationPoint);
                        //' Rotate the original entity
                        using (_operationService.Doc.LockDocument())
                        {
                            var objIds = (ObjectIdCollection)GetObjXData(tCntId);
                            if (objIds.Count == 0)
                                return functionReturnValue;
                            foreach (ObjectId o in objIds)
                            {
                                var en = (Entity)tr.GetObject(o, OpenMode.ForRead, false);
                                try
                                {
                                    en.UpgradeOpen();
                                    en.TransformBy(trans);
                                    SetStoredRotation(en, newAngle);
                                    //' Store the new rotation as XData
                                }
                                catch (Exception ex)
                                {
                                    Log.Add(ex);
                                }
                            }
                        }
                    }
                }
                tr.Commit();
            }
            return newAngle;
            //return functionReturnValue;
        }

        //Extension Method: Add xRecord for Named Object Dictionary to Model Space
        public void XAddDataToNOD(string tNODDataName = "")
        {
            // pick entity to add data to! 
            var trans = _operationService.Ed.Document.Database.TransactionManager.StartTransaction();
            try
            {
                //  Here we will add our data to the Named Objects Dictionary.(NOD) 
                var nod = (DBDictionary)trans.GetObject(_operationService.Ed.Document.Database.NamedObjectsDictionaryId, OpenMode.ForRead);
                if (nod.Contains("MyData"))
                {
                    // Check to see if our entry is in Named Objects Dictionary. 
                    var entryId = nod.GetAt("MyData");

                    // If we are here, then the Name Object Dictionary already has our data 
                    _operationService.Ed.WriteMessage("\n" + "This entity already has data...");

                    // Open the Xrecord for read 
                    var myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                    // Print out the values of the Xrecord to the command line. 
                    foreach (var value in myXrecord.Data)
                    {
                        // Use the WriteMessage method of the editor. 
                        _operationService.Ed.WriteMessage("\n" + Convert.ToString(value.TypeCode) + " . " + Convert.ToString(value.Value));
                    }
                }
                else
                {
                    // Our data is not in the Named Objects Dictionary so need to add it 
                    nod.UpgradeOpen();

                    // Declare a varable as a new Xrecord. 
                    var myXrecord = new Xrecord();

                    // Create the resbuf list. 
                    var data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                        new TypedValue((int)DxfCode.Text, "MyCompanyDefaultSettings"),
                        new TypedValue((int)DxfCode.Real, 51.9),
                        new TypedValue((int)DxfCode.Real, 100.0),
                        new TypedValue((int)DxfCode.Real, 320.6));

                    //  Add the ResultBuffer to the Xrecord 
                    myXrecord.Data = data;

                    // Create the entry in the ExtensionDictionary. 
                    nod.SetAt("MyData", myXrecord);

                    // Tell the transaction about the newly created Xrecord 
                    trans.AddNewlyCreatedDBObject(myXrecord, true);
                }

                // all ok, commit it 
                trans.Commit();
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            finally
            {
                // whatever happens we must dispose the transaction 
                trans.Dispose();
            }
        }

        #region <Block operations: Get, Read, Remove, Update>

        //Update BlockAttribute by BlockName
        public void UpdateBlockAttributeByName(string tBlockName, string tAttrName, string tNewValue)
        {
            try
            {
                using (var tr = _operationService.Db.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)tr.GetObject(_operationService.Db.BlockTableId, OpenMode.ForRead);
                    if (bt.Has(tBlockName))
                    {
                        var btr = (BlockTableRecord)bt[tBlockName].GetObject(OpenMode.ForRead);
                        foreach (ObjectId objId in btr.GetBlockReferenceIds(true, true))
                        {
                            //var br = (BlockReference)tr.GetObject(objId, OpenMode.ForRead);
                            UpdateBlockAttributeByObjectId(objId, tAttrName, tNewValue);
                        }
                    }
                    else
                    {
                        Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Block not found!");
                    }
                    tr.Commit();
                }
                //For Each ObjId As ObjectId In BlkRefs
                //    UpdateAttributesInBlock(ObjId, attbName, attbValue)
                //Next
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
        } //UpdateBlockAttributeByName

        //Update BlockAttribute by BlockId
        private void UpdateBlockAttributeByObjectId(ObjectId tObjId, string tAttrName, string tNewValue)
        {
            using (var tr = _operationService.Db.TransactionManager.StartTransaction())
            {
                var br = (BlockReference)tr.GetObject(tObjId, OpenMode.ForRead);
                foreach (ObjectId arId in br.AttributeCollection)
                {
                    var ar = (AttributeReference)tr.GetObject(arId, OpenMode.ForRead);
                    if ((ar.Tag.ToUpper() != tAttrName.ToUpper())) continue;
                    using (_operationService.Doc.LockDocument())
                    {
                        ar.UpgradeOpen();
                        ar.ColorIndex = 1;
                        ar.TextString = tNewValue;
                        ar.DowngradeOpen();
                    }
                }
                tr.Commit();
                Application.DocumentManager.MdiActiveDocument.Editor.Regen();
                //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(Convert.ToChar(13).ToString(CultureInfo.InvariantCulture));
            }
        } //UpdateBlockAttributeByObjectId

        #endregion

        #region <Object operations: Get, Read, Remove>

        //GetObjectId from Handle
        public ObjectId GetObjectId(string hnd)
        {
            var id = ObjectId.Null;
            try
            {
                _operationService.Db.TryGetObjectId(new Handle(Convert.ToInt64(hnd, 16)), out id);
            }
            catch (System.Exception ex)
            {
                Log.Add(ex);
                //Log.Add("Error", "... Entity handle" + hnd + " format error" + ex.Message).XGetMessage(m => m.Key +", "+ m.Comment);
                _operationService.CurrentObject = null;
            }

            return id;
        }

       

        //GetObject from Handle
        public Entity GetObject(string hnd)
        {
            _operationService.CurrentObject = null;
            try
            {
                ObjectId id;
                if (_operationService.Db.TryGetObjectId(new Handle(Convert.ToInt64(hnd, 16)), out id))
                {
                    using (var tr = _operationService.Db.TransactionManager.StartTransaction())
                    {
                        _operationService.CurrentObject = (Entity)tr.GetObject(id, OpenMode.ForRead);
                    }
                }
                else
                {
                    Log.Add(new Exception(ErrorStatus.UnknownHandle, "... Entity handle" + hnd + " not found"));
                    //Log.Add("Error", "... Entity handle" + hnd + " not found").XGetMessage();
                    _operationService.CurrentObject = null;
                }

            }
            catch (System.Exception ex)
            {
                Log.Add(ex);
                //Log.Add("Error", "... Entity handle" + hnd + " format error. " + ex.Message).XGetMessage();
                _operationService.CurrentObject = null;
            }

            return _operationService.CurrentObject;
        }

        //Remove object by Handle
        public void RemoveObject(string hnd)
        {
            RemoveObject(GetObject(hnd).ObjectId);
        }

        //Remove object by ObjectId
        public void RemoveObject(ObjectId objId)
        {
            using (_operationService.Doc.LockDocument())
            {
                using (var trans = _operationService.Db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        var ent = (Entity)_operationService.Db.TransactionManager.GetObject(objId, OpenMode.ForWrite, true);
                        ent.Erase();
                        ent.Dispose();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Log.Add(ex); //Tools.Ed.WriteMessage(MethodBase.GetCurrentMethod().Name, ex.Message);
                    }
                }
            }
        }

        public void RemoveObjects(string[] tLayerObject)
        {
            var db = _operationService.Db;
            using (_operationService.Doc.LockDocument())
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    foreach (ObjectId entId in btr)
                    {
                        var ent = (Entity)tr.GetObject(entId, OpenMode.ForWrite);
                        if (tLayerObject != null && tLayerObject.Length != 0)
                        {
                            //var arrlstTemp = ArrayList.Adapter(tLayerObject);
                            if (tLayerObject.Contains(ent.Layer))
                            {
                                ent.Erase(true);
                            }
                        }
                        else
                        {
                            ent.Erase(true);
                        }
                    }
                    tr.Commit();
                }
            }
        }

        public void EraseObject()
        {
            // Get the current document and database
            var doc = _operationService.Doc;
            var db = _operationService.Db;

            // Start a transaction
            using (var tr = db.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                var acBlkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                var acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                    OpenMode.ForWrite) as BlockTableRecord;

                // Create a lightweight polyline
                var acPoly = new Polyline();
                acPoly.AddVertexAt(0, new Point2d(2, 4), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(4, 2), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(6, 4), 0, 0, 0);

                // Add the new object to the block table record and the transaction
                if (acBlkTblRec != null) acBlkTblRec.AppendEntity(acPoly);
                tr.AddNewlyCreatedDBObject(acPoly, true);

                // Update the display and display an alert message
                doc.Editor.Regen();
                Application.ShowAlertDialog("Erase the newly added polyline.");

                // Erase the polyline from the drawing
                acPoly.Erase(true);

                // Save the new object to the database
                tr.Commit();
            }
        }

        // GetObject from Handle string
        public Entity GetObjectByHandle(string handle)
        {
            Entity currentObject;
            try
            {
                currentObject = GetObjectByHandle(Convert.ToInt64(handle, 16));
            }
            catch (Exception ex)
            {
                Log.Add(ex);
                return null;
            }
            return currentObject;
        }

        // GetObject from Handle Long
        public Entity GetObjectByHandle(long handle)
        {
            Entity currentObject;
            try
            {
                ObjectId id;
                if (_operationService.Db.TryGetObjectId(new Handle(handle), out id))
                {
                    using (var tr = _operationService.Db.TransactionManager.StartTransaction())
                    {
                        currentObject = (Entity)tr.GetObject(id, OpenMode.ForRead);
                    }
                }
                else
                {
                    var msg = "... Entity handle" + Convert.ToString(handle) + " not found";
                    Log.Add(new SystemException(msg));
                    currentObject = null;
                }

            }
            catch (Exception ex)
            {
                Log.Add(ex);
                currentObject = null;
            }

            return currentObject;
        }

        // Get ObjectId from Handle
        public ObjectId GetObjectIdByHandle(string handle)
        {
            ObjectId id;
            var h = new Handle(Int64.Parse(handle, NumberStyles.AllowHexSpecifier));
            //New Handle(Convert.ToInt64(Handle, 16))
            _operationService.Db.TryGetObjectId(h, out id);

            return id;
        }

        #endregion

        #region "XData - Get, Set, Update XData"

        const string KRegAppName = "MyCad";
        const int KAppCode = 1001;
        const int KRotCode = 1040;

        public object GetObjXData(int tCntId)
        {
            return GetObjXData(OptionsGetSelect.All, tCntId);
        } //GetObjXData

        public ObjectIdCollection GetObjXData(OptionsGetSelect tPromptOption = OptionsGetSelect.All, int tCntId = -1,
            OptionsGetResult tReturnOption = OptionsGetResult.ObjectIdColl, string tXDataParameter = "CntId")
        {
            var n = 0;
            var buff = new StringBuilder("");
            var retObjIds = new ObjectIdCollection();

            // Clear the pickfirst set...
            Application.DocumentManager.MdiActiveDocument.Editor.Regen();
            using (var tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {

                if (tPromptOption == OptionsGetSelect.GetObject)
                {
                    var opt = new PromptEntityOptions(Convert.ToChar(10) + "Select construction: ");
                    PromptEntityResult res = _operationService.Ed.GetEntity(opt);
                    if (res.Status != PromptStatus.OK)
                    {
                        return null;
                    }
                    var obj = tr.GetObject(res.ObjectId, OpenMode.ForRead);
                    var rb = obj.XData;
                    if (rb == null)
                    {
                        Application.ShowAlertDialog("Entity is not construction" + Convert.ToChar(10));
                        return null;
                        //WorkSpace.Ed.WriteMessage(vbLf & "Entity is not construction")
                    }
                    if (tReturnOption == OptionsGetResult.ToDisplay)
                    {
                        foreach (var tv in obj.XData)
                        {
                            buff = buff.AppendLine(String.Format("{0} - {2}", Math.Max(Interlocked.Increment(ref n), n - 1), tv.TypeCode, tv.Value));
                            //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("{0} - {2}" & vbLf, System.Math.Max(System.Threading.Interlocked.Increment(n), n - 1), tv.TypeCode, tv.Value)
                        }
                        rb.Dispose();
                    }
                }

                if (tPromptOption == OptionsGetSelect.SelectLast)
                {
                    var psr = _operationService.Ed.SelectLast();
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

                if (tReturnOption == OptionsGetResult.ObjectIdColl | tReturnOption == OptionsGetResult.ToDisplay)
                {
                    var bt = (BlockTable)tr.GetObject(Application.DocumentManager.MdiActiveDocument.Database.BlockTableId, OpenMode.ForRead);
                    var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                    foreach (ObjectId objId in btr)
                    {
                        var ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                        if (ent.XData != null)
                        {
                            foreach (TypedValue tv in ent.XData)
                            {
                                if (tReturnOption == OptionsGetResult.ToDisplay)
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

                if (tReturnOption == OptionsGetResult.ToDisplay)
                {
                    Application.ShowAlertDialog(buff.ToString());
                }
            }
            return retObjIds;
        } // Get XData

        public void XSetDataByObjectId(ObjectId tObjId, params string[] tParams)
        {
            var tr = _operationService.Doc.TransactionManager.StartTransaction();
            using (tr)
            {
                using (_operationService.Doc.LockDocument())
                {
                    Entity obj;
                    if (tObjId == ObjectId.Null)
                    {
                        var opt = new PromptEntityOptions("\nSelect construction: "); //Convert.ToChar(10) +
                        PromptEntityResult res = _operationService.Ed.GetEntity(opt);
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
                        AddRegAppTableRecord("MivneCad Constructions");
                        var rb = new ResultBuffer(new TypedValue((int)XDataTypeCode.App, "MivneCad Constructions")) { new TypedValue((int)XDataTypeCode.Data, obj.GetType().ToString()) };
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

        public void UpdateXData(ObjectId tObjId, TypeCode tTypeCode, int tValue)
        {
            using (var tr = _operationService.Doc.TransactionManager.StartTransaction())
            {
                using (_operationService.Doc.LockDocument())
                {
                    Entity obj;
                    if (tObjId == ObjectId.Null)
                    {
                        var opt = new PromptEntityOptions("\nSelect construction: "); //Constants.vbLf
                        PromptEntityResult res = _operationService.Ed.GetEntity(opt);
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
                    var rb = new ResultBuffer(new TypedValue((int)XDataTypeCode.App, "MivneCad Constructions"));
                    rb.Add(new TypedValue((int)tTypeCode, tValue));
                    obj.XData = rb;
                    rb.Dispose();
                }
            }
        } // Update XData

        public void AddRegAppTableRecord(string regAppName)
        {
            using (var tr = _operationService.Doc.TransactionManager.StartTransaction())
            {
                var rat = (RegAppTable)tr.GetObject(_operationService.Db.RegAppTableId, OpenMode.ForRead, false);
                if (!rat.Has(regAppName))
                {
                    using (_operationService.Doc.LockDocument())
                    {
                        rat.UpgradeOpen();
                        var ratr = new RegAppTableRecord { Name = regAppName };
                        rat.Add(ratr);
                        tr.AddNewlyCreatedDBObject(ratr, true);
                        _pluginManager.IsRegAppTable = true;
                    }
                }
                tr.Commit();
            }
        }

        // Store our rotation angle as XData
        public void SetStoredRotation(DBObject obj, double rotation)
        {
            AddRegAppTableRecord(KRegAppName);
            var rb = obj.XData;
            if (rb == null)
            {
                rb = new ResultBuffer(new TypedValue(KAppCode, KRegAppName), new TypedValue(KRotCode, rotation));
            }
            else
            {
                // We can simply add our values - no need
                // to remove the previous ones, the new ones
                // are the ones that get stored
                var rbNew = new ResultBuffer();
                foreach (var typedVal in obj.XData)
                {
                    rbNew.Add(typedVal.Value.ToString().Contains("Rotate=")
                                  ? new TypedValue(typedVal.TypeCode, "Rotate=" + Convert.ToString(rotation))
                                  : new TypedValue(typedVal.TypeCode, typedVal.Value));
                }
                obj.XData = rbNew;
                rbNew.Dispose();
            }
            rb.Dispose();
        } // SetStoredRotation

        // Retrieve the existing rotation angle from XData
        public double GetStoredRotation(DBObject obj)
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
                    if (tv.TypeCode == KAppCode)
                    {
                        bReadyForRot = true;
                    }
                }
                rb.Dispose();
            }
            return ret;
        } // GetStoredRotation

        #endregion

        #region "Jig"

        public class PlineJig : EntityJig
        {
            // Maintain a list of vertices...
            // Not strictly necessary, as these will be stored in the
            // polyline, but will not adversely impact performance
            private Point3dCollection _pts;
            // Use a separate variable for the most recent point...
            // Again, not strictly necessary, but easier to reference
            private Point3d _tempPoint;

            private Plane _plane;
            public PlineJig(Matrix3d ucs)
                : base(new Polyline())
            {
                // Create a point collection to store our vertices
                _pts = new Point3dCollection();
                // Create a temporary plane, to help with calcs
                var origin = new Point3d(0, 0, 0);
                var normal = new Vector3d(0, 0, 1);
                normal = normal.TransformBy(ucs);
                _plane = new Plane(origin, normal);
                // Create polyline, set defaults, add dummy vertex
                var pline = (Polyline)Entity;
                pline.SetDatabaseDefaults();
                pline.Normal = normal;
                pline.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var jigOpts = new JigPromptPointOptions
                                  {
                                      UserInputControls =
                                          (UserInputControls.Accept3dCoordinates | UserInputControls.NullResponseAccepted |
                                           UserInputControls.NoNegativeResponseAccepted)
                                  };
                if (_pts.Count == 0)
                {
                    // For the first vertex, just ask for the point
                    jigOpts.Message = "\nStart point of polyline: ";
                }
                else if (_pts.Count > 0)
                {
                    // For subsequent vertices, use a base point
                    jigOpts.BasePoint = _pts[_pts.Count - 1];
                    jigOpts.UseBasePoint = true;
                    jigOpts.Message = "\nPolyline vertex: ";
                }
                else
                {
                    // should never happen
                    return SamplerStatus.Cancel;
                }
                // Get the point itself
                var res = prompts.AcquirePoint(jigOpts);
                // Check if it has changed or not
                // (reduces flicker)
                if (_tempPoint == res.Value)
                {
                    return SamplerStatus.NoChange;
                }
                if (res.Status == PromptStatus.OK)
                {
                    _tempPoint = res.Value;
                    return SamplerStatus.OK;
                }
                return SamplerStatus.Cancel;
            }

            protected override bool Update()
            {
                // Update the dummy vertex to be our
                // 3D point projected onto our plane
                var pline = (Polyline)Entity;
                pline.SetPointAt(pline.NumberOfVertices - 1, _tempPoint.Convert2d(_plane));
                return true;
            }

            public Entity GetEntity()
            {
                return Entity;
            }

            public void AddLatestVertex()
            {
                // Add the latest selected point to our internal list...
                // This point will already be in the most recently added pline vertex
                _pts.Add(_tempPoint);
                var pline = (Polyline)Entity;
                // Create a new dummy vertex... can have any initial value
                pline.AddVertexAt(pline.NumberOfVertices, new Point2d(0, 0), 0, 0, 0);
            }

            public void RemoveLastVertex()
            {
                // Let's remove our dummy vertex
                var pline = (Polyline)Entity;
                pline.RemoveVertexAt(_pts.Count);
            }
        }

        public class RotateJig : EntityJig
        {
            // Declare some internal state
            private double _baseAngle;
            private double _deltaAngle;
            private Point3d _rotationPoint;
            private Matrix3d _ucs;
            // Constructor sets the state and clones
            // the entity passed in
            // (adequate for simple entities)
            public RotateJig(Entity ent, Point3d rotationPoint, double baseAngle, Matrix3d ucs)
                : base(ent.Clone() as Entity)
            {
                _rotationPoint = rotationPoint;
                _baseAngle = baseAngle;
                _ucs = ucs;
            }

            protected override SamplerStatus Sampler(JigPrompts jp)
            {
                // We acquire a single angular value
                var jo = new JigPromptAngleOptions("\nAngle of rotation: ") { BasePoint = _rotationPoint, UseBasePoint = true };
                //Dim jo As New JigPromptDistanceOptions(vbLf & "Distance: ")
                var pdr = jp.AcquireAngle(jo);
                //Dim pdr As PromptDoubleResult = jp.AcquireDistance(jo)
                if (pdr.Status == PromptStatus.OK)
                {
                    // Check if it has changed or not
                    // (reduces flicker)
                    if (Math.Abs(_baseAngle - pdr.Value) < 0.001)
                    {
                        return SamplerStatus.NoChange;
                    }
                    // Set the change in angle to
                    // the new value
                    _deltaAngle = pdr.Value;
                    return SamplerStatus.OK;
                }
                return SamplerStatus.Cancel;
            }

            protected override bool Update()
            {
                // Filter out the case where a zero delta is provided
                if (_deltaAngle > Tolerance.Global.EqualPoint)
                {
                    // We rotate the polyline by the change
                    // minus the base angle
                    var trans = Matrix3d.Rotation(_deltaAngle - _baseAngle, _ucs.CoordinateSystem3d.Zaxis, _rotationPoint);
                    Entity.TransformBy(trans);
                    // The base becomes the previous delta
                    // and the delta gets set to zero
                    _baseAngle = _deltaAngle;
                    _deltaAngle = 0.0;
                }
                return true;
            }

            public Entity GetEntity()
            {
                return Entity;
            }

            public double GetRotation()
            {
                // The overall rotation is the
                // base plus the delta
                return _baseAngle + _deltaAngle;
            }
        }

        public class LineJig : EntityJig
        {
            private Point3dCollection _pts;
            // Use a separate variable for the most recent point...
            // Again, not strictly necessary, but easier to reference
            private Point3d _tempPoint;
            private Plane _plane;
            public LineJig(Matrix3d ucs, Point3d? pt1 = null, Point3d? pt2 = null)
                : base(new Line())
            {
                // Create a point collection to store our vertices
                _pts = new Point3dCollection();
                if (pt1 != null)
                    _pts.Add((Point3d)pt1);
                if (pt2 != null)
                    _pts.Add((Point3d)pt2);
                // Create a temporary plane, to help with calcs
                var origin = new Point3d(0, 0, 0);
                var normal = new Vector3d(0, 0, 1);
                normal = normal.TransformBy(ucs);
                _plane = new Plane(origin, normal);
                // Create line, set defaults, add dummy vertex
                var line = (Line)Entity;
                line.SetDatabaseDefaults();
                line.Normal = normal;
                if (pt1 != null)
                    line.StartPoint = (Point3d)pt1;
                //line.EndPoint = pt2
                //MsgBox("1:" + line.Angle.ToString)
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var jigOpts = new JigPromptPointOptions
                                  {
                                      UserInputControls =
                                          (UserInputControls.Accept3dCoordinates |
                                           UserInputControls.NullResponseAccepted |
                                           UserInputControls.NoNegativeResponseAccepted)
                                  };
                switch (_pts.Count)
                {
                    case 0:
                        jigOpts.Message = "\nStart point of Construction: ";
                        break;
                    case 1:
                        jigOpts.BasePoint = _pts[_pts.Count - 1];
                        jigOpts.UseBasePoint = true;
                        jigOpts.Message = "\nSecond point of Construction: ";
                        break;
                    default:
                        return (SamplerStatus.Cancel);
                }
                // Get the point itself
                var res = prompts.AcquirePoint(jigOpts);
                // Check if it has changed or not
                // (reduces flicker)
                if (_tempPoint == res.Value)
                {
                    return SamplerStatus.NoChange;
                }
                if (res.Status == PromptStatus.OK)
                {
                    _tempPoint = res.Value;
                    //If m_pts.Count < 1 Then
                    return SamplerStatus.OK;
                    //End If
                }
                return SamplerStatus.Cancel;
            }
            protected override bool Update()
            {
                var line = (Line)Entity;
                if (_pts.Count == 1)
                    line.StartPoint = _pts[0];
                if (_pts.Count == 2)
                    line.EndPoint = new Point3d(_pts[0].X, _tempPoint.Convert2d(_plane).X, 0);
                return true;
            }
            public double GetAngle()
            {
                _pts.Add(_tempPoint);
                var line = (Line)Entity;
                line.StartPoint = _pts[0];
                line.EndPoint = _pts[1];
                Application.ShowAlertDialog("GetAngle:" + Convert.ToString(line.Angle));
                return line.Angle;
            }
        }

        public class TextJig : DrawJig
        {
            public Point3d Position { get; private set; }

            // We'll keep our style alive rather than recreating it
            private readonly TextStyle _style;
            public TextJig()
            {
                _style = new TextStyle
                             {
                                 Font =
                                     new FontDescriptor("Calibri", false, true, 0, 0),
                                 TextSize = 10
                             };
            }
            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var opts = new JigPromptPointOptions { UserInputControls = UserInputControls.Accept3dCoordinates, Message = "\nSelect point: " };
                var res = prompts.AcquirePoint(opts);
                if (res.Status == PromptStatus.OK)
                {
                    if (Position == res.Value)
                    {
                        return SamplerStatus.NoChange;
                    }
                    Position = res.Value;
                    return SamplerStatus.OK;
                }
                return SamplerStatus.Cancel;
            }
            protected override bool WorldDraw(WorldDraw draw)
            {
                if (draw == null) throw new ArgumentNullException("draw");
                // We make use of another interface to push our transforms
                var wg2 = draw.Geometry;
                if (wg2 != null)
                {
                    // Push our transforms onto the stack
                    wg2.PushOrientationTransform(OrientationBehavior.Screen);
                    wg2.PushPositionTransform(PositionBehavior.Screen, new Point2d(30, 30));
                    // Draw our screen-fixed text
                    // Position
                    // Normal
                    // Direction
                    // Text
                    // Rawness
                    // TextStyle
                    wg2.Text(new Point3d(0, 0, 0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), Position.ToString(), true, _style);
                    // Remember to pop our transforms off the stack
                    wg2.PopModelTransform();
                    wg2.PopModelTransform();
                }
                return true;
            }

            //protected override bool WorldDraw(WorldDraw draw)
            //{
            //    throw new NotImplementedException();
            //}
        }

        public class ThreePtCircleJig : EntityJig
        {
            private Point3d _first;
            private Point3d _second;
            private Point3d _third;
            //       Private rbfResult As Autodesk.AutoCAD.DatabaseServices.ResultBuffer = New ResultBuffer(New TypedValue(CInt(LispDataType.Double), dRadOfTile), _
            //New TypedValue(CInt(LispDataType.Double), dSpaceOfTiles), New TypedValue(CInt(LispDataType.Text), strCreationType), _
            //New TypedValue(CInt(LispDataType.Text), strPlineType))
            //Private ppp As New Polyline(2) With {.XData = "Beam"}
            public ThreePtCircleJig(Point3d first, Point3d second)
                : base(new Polyline(2))
            {
                _first = first;
                _second = second;
            }
            protected override SamplerStatus Sampler(JigPrompts jp)
            {
                // We acquire a single 3D point
                var jo = new JigPromptPointOptions("\nSelect third point") { UserInputControls = UserInputControls.Accept3dCoordinates };
                var ppr = jp.AcquirePoint(jo);
                if (ppr.Status == PromptStatus.OK)
                {
                    // Check whether it's basically unchanged
                    if (_third.DistanceTo(ppr.Value) < Tolerance.Global.EqualPoint)
                    {
                        return SamplerStatus.NoChange;
                    }
                    // Otherwise just set the jig's state
                    _third = ppr.Value;
                    return SamplerStatus.OK;
                }
                return SamplerStatus.Cancel;
            }
            protected override bool Update()
            {
                // Create a temporary CircularArc3d by three points
                // and use it to create our Circle
                var ca = new CircularArc3d(_first, _second, _third);
                var cir = (Circle)Entity;
                cir.Center = ca.Center;
                cir.Normal = ca.Normal;
                cir.Radius = ca.Radius;
                return true;
            }
            public Entity GetEntity()
            {
                return Entity;
            }
        }

        public class InsertJig : EntityJig
        {
            public InsertJig(ObjectId blockId, Point3d position, Vector3d normal)
                : base(new BlockReference(position, blockId))
            {
                BlockReference.Normal = normal;
                //position = position;
            }
            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var jigOpts = new JigPromptPointOptions
                                  {
                                      UserInputControls = UserInputControls.Accept3dCoordinates,
                                      Message = "" + "\nInsertion point: "
                                  };
                var res = prompts.AcquirePoint(jigOpts);
                var curPoint = res.Value;
                if (_position.DistanceTo(curPoint) > 0.0001)
                {
                    _position = curPoint;
                }
                else
                {
                    return SamplerStatus.NoChange;
                }
                return res.Status == PromptStatus.Cancel ? SamplerStatus.Cancel : SamplerStatus.OK;
            }
            protected override bool Update()
            {
                try
                {
                    if (BlockReference.Position.DistanceTo(_position) > 0.0001)
                    {
                        BlockReference.Position = _position;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Add(ex);
                    Application.ShowAlertDialog(ex.Message);
                }
                return false;
            }
            public BlockReference BlockReference
            {
                get { return (BlockReference)Entity; }
            }
            private Point3d _position;
        }

        public class RectangleJig : EntityJig
        {
            private readonly Point2d _p1;
            private Point2d _p2;
            private Point2d _p3;
            private Point2d _p4;
            public Point3d Corner1;
            public Point3d Corner2;
            private PromptPointResult _pres;
            //Math.Tan((30.0 / 180.0) * Math.PI)
            private const double tan30 = 0;

            public Polyline Pline;
            //Public PointsList As New List(Of Point3d)

            public RectangleJig(Point3d corner1)
                : base(new Polyline(3))
            {
                Corner1 = corner1;

                //PointsList.Add(corner1)

                Pline = (Polyline)Entity;
                Pline.SetDatabaseDefaults();
                _p1 = new Point2d(Corner1.X, Corner1.Y);
                Pline.AddVertexAt(0, _p1, 0, 0, 0);
                Pline.AddVertexAt(1, _p1, 0, 0, 0);
                Pline.AddVertexAt(2, _p1, 0, 0, 0);
                Pline.AddVertexAt(3, _p1, 0, 0, 0);
                Pline.Closed = true;
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var jigPointOpts = new JigPromptPointOptions("\nSpecify other corner point");

                jigPointOpts.UseBasePoint = true;
                jigPointOpts.BasePoint = Corner1;
                jigPointOpts.UserInputControls = (UserInputControls.Accept3dCoordinates) | UserInputControls.NullResponseAccepted;
                _pres = prompts.AcquirePoint(jigPointOpts);

                var endPointTemp = _pres.Value;
                if (endPointTemp != Corner2)
                {
                    Corner2 = endPointTemp;
                }
                else
                {
                    return SamplerStatus.NoChange;
                }

                if (_pres.Status == PromptStatus.Cancel)
                {
                    return SamplerStatus.Cancel;
                }
                //PointsList.Add(corner2)
                return SamplerStatus.OK;
            }

            protected override bool Update()
            {
                _p3 = new Point2d(Corner2.X, Corner2.Y);
                double y = tan30 * (Corner2.X - Corner1.X);
                _p2 = new Point2d(Corner2.X, Corner1.Y + y);
                _p4 = new Point2d(Corner1.X, Corner2.Y - y);
                Pline.SetPointAt(1, _p2);
                Pline.SetPointAt(2, _p3);
                Pline.SetPointAt(3, _p4);

                return true;
            }

        }

        public class RectangleJig1 : EntityJig
        {
            private Point2d P1;
            private Point2d P2;
            private Point2d P3;
            private Point2d P4;
            private readonly Point3d _corner1;
            private Point3d _corner2;
            private PromptPointResult _pres;
            public Polyline Pline;
            private double _tan30 = Math.Tan((30.0 / 180.0) * Math.PI);
            private double _degAng2Use1;

            private double _degAng2Use2;

            public RectangleJig1()
                : this(new Point3d())
            {
            }

            public RectangleJig1(Point3d corner1)
                : base(new Polyline(3))
            {
                _corner1 = corner1;
                Pline = (Polyline)Entity;
                Pline.SetDatabaseDefaults();
                P1 = new Point2d(_corner1.X, _corner1.Y);
                Pline.AddVertexAt(0, P1, 0, 0, 0);
                Pline.AddVertexAt(1, P1, 0, 0, 0);
                Pline.AddVertexAt(2, P1, 0, 0, 0);
                Pline.AddVertexAt(3, P1, 0, 0, 0);
                Pline.Closed = true;
            }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                var jigPointOpts = new JigPromptPointOptions("\nSpecify other corner point")
                                       {
                                           UseBasePoint = true,
                                           BasePoint = _corner1,
                                           UserInputControls =
                                               (UserInputControls.Accept3dCoordinates) |
                                               UserInputControls.NullResponseAccepted
                                       };

                _pres = prompts.AcquirePoint(jigPointOpts);
                var endPointTemp = _pres.Value;

                if (endPointTemp != _corner2)
                {
                    _corner2 = endPointTemp;
                }
                else
                {
                    return SamplerStatus.NoChange;
                }

                if (_pres.Status == PromptStatus.Cancel)
                {
                    return SamplerStatus.Cancel;
                }
                return SamplerStatus.OK;
            }

            private bool IsometricSnapIsOn()
            {
                return Application.GetSystemVariable("Snapstyl").ToString() == "1";
            }

            private String GetIsoPlane()
            {
                var result = "Left";

                switch (Application.GetSystemVariable("Snapisopair").ToString())
                {
                    case "1":
                        result = "Top";
                        break;
                    case "2":
                        result = "Right";
                        break;
                }
                return result;
            }

            private Point2d PolarPoint(Point2d basepoint, double angle, double distance)
            {
                return new Point2d(basepoint.X + (distance * Math.Cos(angle)), basepoint.Y + (distance * Math.Sin(angle)));
            }

            private Point2d ImaginaryIntersect(Point2d line1Pt1, Point2d line1Pt2, Point2d line2Pt1, Point2d line2Pt2)
            {
                var line1 = new Line2d(line1Pt1, line1Pt2);
                var line2 = new Line2d(line2Pt1, line2Pt2);

                var line1Ang = line1.Direction.Angle;
                var line2Ang = line2.Direction.Angle;

                var line1ConAng = line1Ang + DegreesRadiansConversion(180, false);
                var line2ConAng = line2Ang + DegreesRadiansConversion(180, false);

                var rayLine1Pt1 = PolarPoint(line1Pt1, line1Ang, 10000);
                var rayLine1Pt2 = PolarPoint(line1Pt1, line1ConAng, 10000);
                var rayLine1 = new Line2d(rayLine1Pt1, rayLine1Pt2);

                var rayLine2Pt1 = PolarPoint(line2Pt1, line2Ang, 10000);
                var rayLine2Pt2 = PolarPoint(line2Pt1, line2ConAng, 10000);
                var rayLine2 = new Line2d(rayLine2Pt1, rayLine2Pt2);

                var col = rayLine1.IntersectWith(rayLine2);

                return col[0];

            }

            private Double DegreesRadiansConversion(Double angle, bool inputIsRadians)
            {
                if (inputIsRadians)
                {
                    angle = (180 * (angle / Math.PI));
                }
                else
                {
                    angle = (Math.PI * (angle / 180));
                }
                return angle;
            }

            protected override bool Update()
            {
                if (!IsometricSnapIsOn())
                {
                    _degAng2Use1 = 0;
                    _degAng2Use2 = 90;
                }
                else if (GetIsoPlane() == "Right")
                {
                    _degAng2Use1 = 30;
                    _degAng2Use2 = 90;
                }
                else if (GetIsoPlane() == "Left")
                {
                    if (true)
                    {
                        _degAng2Use1 = 330;
                        _degAng2Use2 = 90;
                    }
                }
                else
                {
                    if (true)
                    {
                        _degAng2Use1 = 30;
                        _degAng2Use2 = 330;
                    }
                }

                var ang2Use1 = DegreesRadiansConversion(_degAng2Use1, false);
                var conAng2Use1 = ang2Use1 + DegreesRadiansConversion(180, false);

                var ang2Use2 = DegreesRadiansConversion(_degAng2Use2, false);
                var conAng2Use2 = ang2Use2 + DegreesRadiansConversion(180, false);

                P3 = new Point2d(_corner2.X, _corner2.Y);
                //double y = tan30 * (corner2.X - corner1.X);
                P2 = ImaginaryIntersect(P1, PolarPoint(P1, ang2Use1, 1), P3, PolarPoint(P3, ang2Use2, 1));
                P4 = ImaginaryIntersect(P1, PolarPoint(P1, conAng2Use2, 1), P3, PolarPoint(P3, conAng2Use1, 1));
                Pline.SetPointAt(1, P2);
                Pline.SetPointAt(2, P3);
                Pline.SetPointAt(3, P4);

                return true;
            }

        }

        //Public Shared Sub Rectangle()
        //    Dim ppo As New PromptPointOptions(vbLf & "Specify first corner point: ")
        //    Dim ppr As PromptPointResult = WorkSpace.Ed.GetPoint(ppo)
        //    If ppr.Status <> PromptStatus.OK Then
        //        Return
        //    End If
        //    Dim corner1 As Point3d = ppr.Value
        //    Dim Recjig As New RectangleJig(corner1)
        //    If WorkSpace.Ed.Drag(Recjig).Status = PromptStatus.Cancel Then
        //        Return
        //    End If

        //    Dim pline As Autodesk.AutoCAD.DatabaseServices.Polyline = Recjig.pline
        //    Using tr As Transaction = WorkSpace.Db.TransactionManager.StartTransaction()
        //        Dim bt As BlockTable = TryCast(tr.GetObject(WorkSpace.Db.BlockTableId, OpenMode.ForRead), BlockTable)
        //        Dim btr As BlockTableRecord = TryCast(tr.GetObject(WorkSpace.Db.CurrentSpaceId, OpenMode.ForWrite), BlockTableRecord)
        //        btr.AppendEntity(pline)
        //        tr.AddNewlyCreatedDBObject(pline, True)
        //        tr.Commit()
        //    End Using
        //    Return
        //End Sub

        #endregion

        public void ExplodeBlock(string blockName)
        {
            using (var tr = _operationService.Db.TransactionManager.StartTransaction())
            {
                var bt = (BlockTable)tr.GetObject(_operationService.Db.BlockTableId, OpenMode.ForRead, false, true);
                if (bt.Has(blockName))
                {
                    var btrid = bt[blockName];
                    if (!btrid.IsEffectivelyErased)
                    {
                        var btr = (BlockTableRecord)tr.GetObject(btrid, OpenMode.ForRead, false, true);

                        var brefIds = btr.GetBlockReferenceIds(true, false);
                        var oids = new ObjectId[brefIds.Count];
                        brefIds.CopyTo(oids, 0);
                        foreach (ObjectId objid in brefIds)
                        {
                            var br = (BlockReference)tr.GetObject(objid, OpenMode.ForWrite);
                            br.ExplodeToOwnerSpace();
                        }
                    }
                }
            }
        }
    }
}
