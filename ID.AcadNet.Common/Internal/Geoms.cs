using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Extensions;
using System;
using System.Collections.Generic;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Intellidesk.AcadNet.Common.Internal
{
    public static class Geoms
    {
        public static Editor Ed
        {
            get { return Doc.Editor; }
        }

        public static Database Db
        {
            get { return Doc.Database; }
        }

        public static Document Doc
        {
            get { return acadApp.DocumentManager.MdiActiveDocument; }
        }

        /// <summary> Degree to Radian </summary>
        public static double DegreeToRadian(int angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary> Radian to Degree </summary>
        public static double RadianToDegree(double radian)
        {
            return (int)(radian * (180.0 / Math.PI));
        }

        public static void GetIntersect()
        {
            try
            {
                using (var tr = Ed.Document.Database.TransactionManager.StartTransaction())
                {
                    var opt = new PromptEntityOptions("\nSelect first polyline: ");
                    opt.SetRejectMessage("\nSelect polyline only");
                    opt.AllowNone = false;
                    opt.AllowObjectOnLockedLayer = true;
                    opt.AddAllowedClass(typeof(Polyline), true);

                    var res = Ed.GetEntity(opt);
                    if (res.Status != PromptStatus.OK) return;
                    var lid = res.ObjectId;
                    opt.AddAllowedClass(typeof(Polyline), true);
                    opt.SetRejectMessage("\nSelect polyline only");
                    opt.Message = "\nSelect second polyline: ";
                    res = Ed.GetEntity(opt);

                    if (res.Status != PromptStatus.OK) return;
                    var pid = res.ObjectId;
                    var pts = new Point3dCollection();
                    Ed.WriteMessage("\n{0} <> {1};", lid, pid);
                    var enl = (Polyline)tr.GetObject(lid, OpenMode.ForRead);
                    var enp = (Polyline)tr.GetObject(pid, OpenMode.ForRead);
                    //var plane = new Plane(WorkSpace.Ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Origin, WorkSpace.Ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Zaxis);
                    //enl.IntersectWith(enp, Intersect.OnBothOperands, plane, pts, 0, 0);
                    enl.IntersectWith(enp, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                    Ed.WriteMessage("\nNumber of intersections: {0}", pts.Count);
                    var i = 1;
                    foreach (Point3d pt in pts)
                    {
                        Ed.WriteMessage("\nPoint number {0}: ({1}, {2}, {3})", i, pt.X, pt.Y, pt.Z);
                        i += 1;
                    }

                    tr.Commit();

                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(Geoms)}.{nameof(GetIntersect)} error: ", ex);
            }
        }

        // Transform from UCS to WCS
        public static Matrix3d UcsToWcs()
        {
            Matrix3d ucs = Ed.CurrentUserCoordinateSystem;
            CoordinateSystem3d cs = ucs.CoordinateSystem3d;
            Matrix3d mat = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis,
                Vector3d.ZAxis, cs.Origin, cs.Xaxis, cs.Yaxis, cs.Zaxis);
            return mat;
        }

        // Translate the OCS to WCS
        public static Matrix3d WorldToPlane(Vector3d vec) //WorldToPlane(acPoly2d.Normal)
        {
            return Matrix3d.WorldToPlane(vec);
        }

        public static Point3d Point3DUcsToWcs(Point3d ucsPoint)
        {
            return ucsPoint.TransformBy(UcsToWcs());
        }

        public static void GetUCS(Database db, Transaction acTrans, Point3d basePt)
        {
            UcsTable acUCSTbl;
            var acDoc = acadApp.DocumentManager.GetDocument(db);
            acUCSTbl = acTrans.GetObject(Db.UcsTableId, OpenMode.ForRead) as UcsTable;

            UcsTableRecord acUCSTblRec = null;
            if (acUCSTbl.Has("WGS84_Old") == false)
            {
                acUCSTblRec = new UcsTableRecord { Name = "WGS84_Old" };
                acUCSTbl.UpgradeOpen();
                acUCSTbl.Add(acUCSTblRec);
                acTrans.AddNewlyCreatedDBObject(acUCSTblRec, true);

                acUCSTblRec = new UcsTableRecord { Name = "ITM" };
                acUCSTbl.Add(acUCSTblRec);
                acTrans.AddNewlyCreatedDBObject(acUCSTblRec, true);

                acUCSTblRec = acTrans.GetObject(acUCSTbl["ITM"], OpenMode.ForWrite) as UcsTableRecord;
                acUCSTblRec.Origin = basePt;
                acUCSTblRec.XAxis = new Vector3d(1, 0, 0);
                acUCSTblRec.YAxis = new Vector3d(0, 1, 0);

                // Open the active viewport
                var acVportTblRec =
                    acTrans.GetObject(acDoc.Editor.ActiveViewportId, OpenMode.ForWrite) as ViewportTableRecord;

                // Display the UCS Icon at the origin of the current viewport
                acVportTblRec.IconAtOrigin = true;
                acVportTblRec.IconEnabled = true;

                // Set the UCS current
                acVportTblRec.SetUcs(acUCSTblRec.ObjectId);
                acDoc.Editor.UpdateTiledViewportsFromDatabase();

                acUCSTblRec.Dispose();
            }
            else
            {
                acUCSTblRec = acTrans.GetObject(acUCSTbl["WGS84_Old"], OpenMode.ForWrite) as UcsTableRecord;
            }

            //acUCSTblRec.XAddXData(new XDataRecord() {Position = basePt});
            if (acUCSTblRec != null)
                db.Ucsname = acUCSTblRec.ObjectId;
        }

        public static bool IsOrthModeOn()
        {
            // Check the value of the ORTHOMODE sysvar

            var orth = acadApp.GetSystemVariable("ORTHOMODE");
            return Convert.ToInt32(orth) > 0;
        }

        public static bool IsInModel()
        {
            if (acadApp.DocumentManager.MdiActiveDocument.Database.TileMode)
                return true;
            else
                return false;
        }

        public static bool IsInLayout()
        {
            return !IsInModel();
        }

        public static bool IsInLayoutPaper()
        {
            Document doc = acadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            if (db.TileMode)
                return false;

            if (db.PaperSpaceVportId == ObjectId.Null)
                return false;

            if (ed.CurrentViewportObjectId == ObjectId.Null)
                return false;

            return ed.CurrentViewportObjectId == db.PaperSpaceVportId;
        }

        public static bool IsInLayoutViewport()
        {
            return IsInLayout() && !IsInLayoutPaper();
        }

        /// <summary>
        /// Represents a quadrilateral (polygon with four points, eg. a rect or a square). Note that the points are supposed
        /// to be in counter-clockwise order (usually starting with the upper left corner).
        /// </summary>
        public struct Quadrilateral
        {
            public Point2d A { get; set; }
            public Point2d B { get; set; }
            public Point2d C { get; set; }
            public Point2d D { get; set; }

            public Quadrilateral(Rectangle3d rect)
                : this()
            {
                A = new Point2d(rect.UpperLeft.X, rect.UpperLeft.Y);
                B = new Point2d(rect.UpperRight.X, rect.UpperRight.Y);
                C = new Point2d(rect.LowerRight.X, rect.LowerRight.Y);
                D = new Point2d(rect.LowerLeft.X, rect.LowerLeft.Y);
            }

            /// <summary>
            /// Rotates the quadrilateral clockwise around the specified point.
            /// </summary>
            /// <param name="degrees">angle in degrees (clockwise)</param>
            /// <param name="center">center point around which the quadrilateral is rotated</param>
            public void Rotate(double degrees, Point2d center)
            {
                //Matrix mat = new Matrix();
                //// NOTE: Matrix.RotateAt() rotates clockwise.
                //mat.RotateAt(degrees, center.X, center.Y);
                //this.A = mat.Transform(this.A);
                //this.B = mat.Transform(this.B);
                //this.C = mat.Transform(this.C);
                //this.D = mat.Transform(this.D);
            }
        }

        public static Extents3d Extents()
        {
            Point3d pMin = Point3d.Origin;
            Point3d pMax = Point3d.Origin;

            if (Db.TileMode)
            {
                if (pMin.Equals(new Point3d()) && pMax.Equals(new Point3d()))
                {
                    pMin = Db.Extmin;
                    pMax = Db.Extmax;
                }
            }
            else
            {
                int nCurVport = Convert.ToInt32(acadApp.GetSystemVariable("CVPORT"));
                // Check to see if Paper space is current
                if (nCurVport == 1)
                {
                    // Get the extents of Paper space
                    if (pMin.Equals(new Point3d()) && pMax.Equals(new Point3d()))
                    {
                        pMin = Db.Pextmin;
                        pMax = Db.Pextmax;
                    }
                }
                else
                {
                    // Get the extents of Model space
                    if (pMin.Equals(new Point3d()) && pMax.Equals(new Point3d()))
                    {
                        pMin = Db.Extmin;
                        pMax = Db.Extmax;
                    }
                }
            }
            return new Extents3d(pMin, pMax);
        }

        public static Extents3d ScreenExtents()
        {
            Point3d screenSizeMin = (Point3d)acadApp.GetSystemVariable("EXTMIN");
            Point3d screenSizeMax = (Point3d)acadApp.GetSystemVariable("EXTMAX");
            return new Extents3d(screenSizeMin, screenSizeMax);
        }

        public static Point2d Point3dToUcs(this Point3d pt, CoordinateSystem3d ucs)
        {
            var pl = new Plane(ucs.Origin, ucs.Zaxis);
            return pl.ParameterOf(pt);
        }

        public static Extents3d GetExtents(this ObjectId[] ids, Transaction tr = null)
        {
            Extents3d extents = new Extents3d();
            using (Doc.LockDocument())
            {
                var tran = tr ?? Db.TransactionManager.StartTransaction();
                List<DBObject> dbObjects = ids.XCast<DBObject>(tran);
                dbObjects.ForEach(x =>
                {
                    if (x.Bounds != null) extents.AddExtents(x.Bounds.Value);
                });
                if (tr == null) tran.Dispose();

                return extents;
            }
        }

        public static Extents3d ObjectIdsExtents_(ObjectIdCollection tObjIds)
        {
            Extents3d result = new Extents3d();
            if (tObjIds.Count > 0)
            {
                try
                {
                    using (Doc.LockDocument())
                    {
                        using (var tr = Db.TransactionManager.StartTransaction())
                        {
                            foreach (ObjectId objId in tObjIds)
                            {
                                if (objId != ObjectId.Null)
                                {
                                    DBObject obj = tr.GetObject(objId, OpenMode.ForRead);
                                    if (obj.Bounds != null)
                                    {
                                        result.AddPoint(obj.Bounds.Value.MinPoint);
                                        result.AddPoint(obj.Bounds.Value.MaxPoint);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Error($"{nameof(Geoms)}.{nameof(ObjectIdsExtents_)} error: ", ex);
                }
            }
            return result;
        }
    }
}