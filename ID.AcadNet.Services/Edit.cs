using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Internal;
using Intellidesk.AcadNet.Common.Jig;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.Data.Models.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Threading;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Intellidesk.AcadNet.Services
{
    public static class Edit
    {
        public static event AcadEntityEventHandler ObjectErased;
        public static event AcadEntityEventHandler ObjectModified;

        public static Editor Ed => Doc.Editor;
        public static Database Db => Doc.Database;
        public static Document Doc => acadApp.DocumentManager.MdiActiveDocument;

        public static bool IsRemakeMode = false;

        #region <Methods>
        public static double RotateCnt(int tCntId = -1, GetSelectOptions tOption = GetSelectOptions.SelectLast)
        {
            const double functionReturnValue = 0;
            double newAngle = 0;
            var objId = ObjectId.Null;

            using (var tr = Db.TransactionManager.StartTransaction())
            {
                while (objId == ObjectId.Null)
                {
                    if (tOption == GetSelectOptions.GetObject | tCntId < 0)
                    {
                        // First we prompt for the entity to rotate
                        objId = Selects.GetEntities(GetSelectOptions.GetObject, "Select object to rotate").FirstOrDefault().ObjectId;
                        if (objId == ObjectId.Null)
                            return functionReturnValue;
                    }
                    else
                    {
                        objId = Doc.XGetXData(GetSelectOptions.All, tCntId)[0];
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
                    if (tOption == GetSelectOptions.GetPoint)
                    {
                        var peo = new PromptPointOptions("")
                        {
                            Message =
                                Convert.ToChar(10).ToString(CultureInfo.InvariantCulture) +
                                "Select point to rotate: "
                        };
                        var per = acadApp.DocumentManager.MdiActiveDocument.Editor.GetPoint(peo);
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
                var baseAngle = obj.GetStoredRotation();

                //EditInputManagement.GetObjXData(obj)
                newAngle = baseAngle;

                if (ent != null)
                {
                    var ucs = Ed.CurrentUserCoordinateSystem;
                    //' Get the current UCS, to pass to the Jig
                    var jig = new RotateJig(ent, rotationPoint, baseAngle, ucs);
                    //' Create our jig object
                    var res = Ed.Drag(jig);
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
                        using (Doc.LockDocument())
                        {
                            var objIds = (ObjectIdCollection)Doc.XGetXData(tCntId);
                            if (objIds.Count == 0)
                                return functionReturnValue;
                            foreach (ObjectId o in objIds)
                            {
                                var en = (Entity)tr.GetObject(o, OpenMode.ForRead, false);
                                try
                                {
                                    en.UpgradeOpen();
                                    en.TransformBy(trans);

                                    IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
                                    Doc.AddRegAppTableRecord(pluginSettings.Name);

                                    en.SetStoredRotation(newAngle);
                                    //' Store the new rotation as XData
                                }
                                catch (Exception ex)
                                {
                                    Plugin.Logger.Error($"{nameof(Edit)}.{nameof(RotateCnt)} error: ", ex);
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

        public static PromptEntityResult PromptForObject(string promptMessage, Type allowedType, bool exactMatchOfAllowedType)
        {
            var polyOptions = new PromptEntityOptions(promptMessage);
            polyOptions.SetRejectMessage("Entity is not of type " + allowedType);
            polyOptions.AddAllowedClass(allowedType, exactMatchOfAllowedType);
            var polyResult = Ed.GetEntity(polyOptions);
            return polyResult;
        }

        public static PromptPointResult PromptForPoint(string promptMessage, bool useDashedLine = false, bool useBasePoint = false, Point3d basePoint = new Point3d(), bool allowNone = true)
        {
            var pointOptions = new PromptPointOptions(promptMessage);
            if (useBasePoint)
            {
                pointOptions.UseBasePoint = true;
                pointOptions.BasePoint = basePoint;
                pointOptions.AllowNone = allowNone;
            }

            if (useDashedLine)
            {
                pointOptions.UseDashedLine = true;
            }
            var pointResult = Ed.GetPoint(pointOptions);
            return pointResult;
        }

        public static PromptPointResult PromptForPoint(PromptPointOptions promptPointOptions)
        {
            return Ed.GetPoint(promptPointOptions);
        }

        public static PromptDoubleResult PromptForDouble(string promptMessage, double defaultValue = 0.0)
        {
            var doubleOptions = new PromptDoubleOptions(promptMessage);
            if (Math.Abs(defaultValue - 0.0) > Double.Epsilon)
            {
                doubleOptions.UseDefaultValue = true;
                doubleOptions.DefaultValue = defaultValue;
            }
            var promptDoubleResult = Ed.GetDouble(doubleOptions);
            return promptDoubleResult;
        }

        public static PromptIntegerResult PromptForInteger(string promptMessage)
        {
            var promptIntResult = Ed.GetInteger(promptMessage);
            return promptIntResult;
        }

        public static PromptResult PromptForKeywordSelection(
            string promptMessage, IEnumerable<string> keywords, bool allowNone, string defaultKeyword = "")
        {
            var promptKeywordOptions = new PromptKeywordOptions(promptMessage) { AllowNone = allowNone };
            foreach (var keyword in keywords)
            {
                promptKeywordOptions.Keywords.Add(keyword);
            }
            if (defaultKeyword != "")
            {
                promptKeywordOptions.Keywords.Default = defaultKeyword;
            }
            var keywordResult = Ed.GetKeywords(promptKeywordOptions);
            return keywordResult;
        }

        public static Point3dCollection PromptForRectangle(out PromptStatus status, string promptMessage)
        {
            var resultRectanglePointCollection = new Point3dCollection();
            var viewCornerPointResult = PromptForPoint(promptMessage);
            var pointPromptStatus = viewCornerPointResult.Status;
            if (viewCornerPointResult.Status == PromptStatus.OK)
            {
                var rectangleJig = new RectangleJig(viewCornerPointResult.Value);
                var jigResult = Ed.Drag(rectangleJig);
                if (jigResult.Status == PromptStatus.OK)
                {
                    // remove duplicate point at the end of the rectangle
                    var polyline = rectangleJig.Polyline;
                    var viewPolylinePoints = polyline.XGetPoints(); //GetPointsFromPolyline(polyline);
                    if (viewPolylinePoints.Count == 5)
                        viewPolylinePoints.RemoveAt(4); // dont know why but true, probably mirror point with the last point
                }
                pointPromptStatus = jigResult.Status;
            }
            status = pointPromptStatus;
            return resultRectanglePointCollection;
        }

        public static PromptSelectionResult PromptForSelection(string promptMessage = null, SelectionFilter filter = null)
        {
            var selectionOptions = new PromptSelectionOptions { MessageForAdding = promptMessage };
            var selectionResult = string.IsNullOrEmpty(promptMessage) ? Ed.SelectAll(filter) : Ed.GetSelection(selectionOptions, filter);
            return selectionResult;
        }

        public static PromptSelectionResult PromptForSelection(PromptSelectionOptions promptSelectionOptions, SelectionFilter filter = null)
        {
            return Ed.GetSelection(promptSelectionOptions, filter);
        }

        public static void WriteMessage(string message)
        {
            Ed.WriteMessage(message);
        }

        public static void DrawVector(Point3d from, Point3d to, int color, bool drawHighlighted)
        {
            Ed.DrawVector(from, to, color, drawHighlighted);
        }

        //public static ObjectId CreateObject<T>(Action<Entity> action = null) where T : Entity
        //{ 
        //    return typeof(T).CreateInstance()
        //}

        public static void UpdateObject(ObjectId ownerObjectId, Action<DBObject> action = null)
        {
            ObjectIdItem result = null;
            using (Doc.LockDocument())
            {
                using (Transaction tr = Db.TransactionManager.StartTransaction())
                {
                    DBObject ent = tr.GetObject(ownerObjectId, OpenMode.ForRead);

                    ent.UpgradeOpen();
                    ent.Erased -= OnObjectErased;
                    ent.Modified -= OnObjectModified;
                    ent.Erased += OnObjectErased;
                    ent.Modified += OnObjectModified;
                    ent.DowngradeOpen();

                    if (action != null)
                    {
                        //ent.XAddXData(element);
                        //result = ent.XGetDisplayItem(element);
                    }

                    tr.Commit();
                    tr.Dispose();
                }
            }
        }

        private static void OnObjectModified(object sender, EventArgs args)
        {
            ObjectModified?.Invoke(sender, args);
        }

        private static void OnObjectErased(object sender, ObjectErasedEventArgs args)
        {
            ObjectErased?.Invoke(sender, args);
        }

        #endregion

        #region <Events>

        public static void OnAcadEntityErased(object sender, EventArgs e)
        {
            var db = HostApplicationServices.WorkingDatabase;
            var dbObject = (DBObject)sender; //db.XGetObject((ObjectId)sender);

            if (!Edit.IsRemakeMode)
            {
                IPaletteElement elementDefinition = dbObject.XGetDataObject();
                if (elementDefinition.Items.Any())
                    db.XRemoveObjects(elementDefinition.Items);
                //db.XRemoveXrecord(dbObject.ExtensionDictionary, dbObject.Handle.ToString());
            }
        }

        public static void OnAcadEntityModified(object sender, EventArgs args)
        {
            DBObject dbObject = (DBObject)sender; //db.XGetObject((DBObject)sender).ObjectId);
            if (dbObject.IsErased || dbObject.IsModifiedGraphics) return;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                if (dbObject is Polyline)
                {
                    var pline = (Polyline)dbObject;
                    //AcadCable acadCable = pline.XGetXDataObject<AcadCable>();
                    //acadCable.DateModified = DateTime.Now;
                    //pline.XAddXData(acadCable);
                    pline.XUpdateDataObject<AcadCable>(acadCable => { acadCable.DateModified = DateTime.Now; return acadCable; });
                }
                else if (dbObject is DBText)
                {
                }
            });
        }

        #endregion
    }
}