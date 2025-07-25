using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Diagnostics;
using System.Windows;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Utils
{
    /// <summary>
    /// General Acad UI Utils used within the AutoCAD environment.
    /// Most of these are just convenient wrappers around existing functions
    /// to make them easier to use.
    /// </summary>
    public class AcadUi
    {
        /// <summary>
        /// simple wrapper to print a string to the AutoCAD command line.  Will not include
        /// a \n unless specifically in the string.
        /// </summary>
        public static void PrintToCmdLine(string str)
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(str);
        }

        /// <summary>
        /// format a point in AutoCAD terminology and style using a specified unit
        /// and precision.
        /// </summary>
        /// <param name="pt">value to format</param>
        /// <param name="unitType">display unit to format as</param>
        /// <param name="prec">number of decimal places for display precision</param>
        public static string PtToStr(Point3d pt, DistanceUnitFormat unitType, int prec)
        {
            string x = Converter.DistanceToString(pt.X, unitType, prec);
            string y = Converter.DistanceToString(pt.Y, unitType, prec);
            string z = Converter.DistanceToString(pt.Z, unitType, prec);

            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        /// <summary>
        /// format a point in AutoCAD terminology and style using the default unit
        /// and precision.
        /// </summary>
        /// <param name="pt">value to format</param>
        public static string PtToStr(Point3d pt)
        {
            return PtToStr(pt, DistanceUnitFormat.Current, -1);
        }

        /// <summary>
        /// format a point in AutoCAD terminology and style using a specified unit
        /// and precision.
        /// </summary>
        /// <param name="pt">value to format</param>
        /// <param name="unitType">display unit to format as</param>
        /// <param name="prec">number of decimal places for display precision</param>
        public static string PtToStr(Point2d pt, DistanceUnitFormat unitType, int prec)
        {
            string x = Converter.DistanceToString(pt.X, unitType, prec);
            string y = Converter.DistanceToString(pt.Y, unitType, prec);

            return string.Format("({0}, {1})", x, y);
        }

        /// <summary>
        /// format a point in AutoCAD terminology and style using the default unit
        /// and precision.
        /// </summary>
        /// <param name="pt">value to format</param>
        public static string PtToStr(Point2d pt)
        {
            return PtToStr(pt, DistanceUnitFormat.Current, -1);
        }

        /// <summary>
        /// format a scale in AutoCAD terminology and style using a specified unit
        /// and precision.
        /// </summary>
        /// <param name="pt">value to format</param>
        /// <param name="unitType">display unit to format as</param>
        /// <param name="prec">number of decimal places for display precision</param>
        public static string ScaleToStr(Scale2d sc, DistanceUnitFormat unitType, int prec)
        {
            string x = Converter.DistanceToString(sc.X, unitType, prec);
            string y = Converter.DistanceToString(sc.Y, unitType, prec);

            return string.Format("({0}, {1})", x, y);
        }

        /// <summary>
        /// format a scale in AutoCAD terminology and style using the default unit
        /// and precision.
        /// </summary>
        /// <param name="pt">value to format</param>
        public static string ScaleToStr(Scale2d sc)
        {
            return ScaleToStr(sc, DistanceUnitFormat.Current, -1);
        }

        /// <summary>
        /// format a scale in AutoCAD terminology and style using a specified unit
        /// and precision.
        /// </summary>
        /// <param name="sc">value to format</param>
        /// <param name="unitType">display unit to format as</param>
        /// <param name="prec">number of decimal places for display precision</param>
        public static string ScaleToStr(Scale3d sc, DistanceUnitFormat unitType, int prec)
        {
            string x = Converter.DistanceToString(sc.X, unitType, prec);
            string y = Converter.DistanceToString(sc.Y, unitType, prec);
            string z = Converter.DistanceToString(sc.Z, unitType, prec);

            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        /// <summary>
        /// format a scale in AutoCAD terminology and style using the default unit
        /// and precision.
        /// </summary>
        /// <param name="sc">value to format</param>
        public static string ScaleToStr(Scale3d sc)
        {
            return ScaleToStr(sc, DistanceUnitFormat.Current, -1);
        }

        /// <summary>
        /// format a vector in AutoCAD terminology and style using a specified unit
        /// and precision.
        /// </summary>
        /// <param name="v">value to format</param>
        /// <param name="unitType">display unit to format as</param>
        /// <param name="prec">number of decimal places for display precision</param>
        public static string VecToStr(Vector3d v, DistanceUnitFormat unitType, int prec)
        {
            string x = Converter.DistanceToString(v.X, unitType, prec);
            string y = Converter.DistanceToString(v.Y, unitType, prec);
            string z = Converter.DistanceToString(v.Z, unitType, prec);

            return string.Format("({0}, {1}, {2})", x, y, z);
        }


        /// <summary>
        /// format a vector in AutoCAD terminology and style using the default unit
        /// and precision.
        /// </summary>
        /// <param name="v">value to format</param>
        public static string VecToStr(Vector3d v)
        {
            return VecToStr(v, DistanceUnitFormat.Current, -1);
        }

        /// <summary>
        /// format a vector in AutoCAD terminology and style using a specified unit
        /// and precision.
        /// </summary>
        /// <param name="v">value to format</param>
        /// <param name="unitType">display unit to format as</param>
        /// <param name="prec">number of decimal places for display precision</param>
        public static string VecToStr(Vector2d v, DistanceUnitFormat unitType, int prec)
        {
            string x = Converter.DistanceToString(v.X, unitType, prec);
            string y = Converter.DistanceToString(v.Y, unitType, prec);

            return string.Format("({0}, {1})", x, y);
        }

        /// <summary>
        /// format a vector in AutoCAD terminology and style using the default unit
        /// and precision.
        /// </summary>
        /// <param name="v">value to format</param>
        public static string VecToStr(Vector2d v)
        {
            return VecToStr(v, DistanceUnitFormat.Current, -1);
        }

        /// <summary>
        /// print out the class name of a given RXObject
        /// </summary>
        /// <param name="obj">RXObject to print the class name from</param>
        public static string ObjToRxClassStr(RXObject obj)
        {
            Debug.Assert(obj != null);

            RXClass rxClass = obj.GetRXClass();
            if (rxClass == null)
            {
                Debug.Assert(false);
                MessageBox.Show("AcRxObject class has not called rxInit()!");
                return "*Unknown*";
            }
            else
                return rxClass.Name;
        }

        public static string ObjToRxClassAndHandleStr(DBObject dbObj)
        {
            Debug.Assert(dbObj != null);

            string str1 = ObjToRxClassStr(dbObj);
            return string.Format("< {0,-25} {1,4} >", str1, dbObj.Handle.ToString());
        }

        public static string ObjToRxClassAndHandleStr(ObjectId objId)
        {
            string str;

            if (objId.IsNull)
                str = "(null)";
            else
            {
                // open up even if erased
                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = objId.Database.TransactionManager;
                using (Autodesk.AutoCAD.DatabaseServices.Transaction tr = tm.StartTransaction())
                {
                    DBObject tmpObj = tr.GetObject(objId, OpenMode.ForRead, true);
                    str = ObjToRxClassAndHandleStr(tmpObj);
                    tr.Commit();
                }
            }

            return str;
        }

        public static string ObjToTypeAndHandleStr(DBObject dbObj)
        {
            Debug.Assert(dbObj != null);

            string str1 = dbObj.GetType().Name;
            return string.Format("< {0,-20} {1,4} >", str1, dbObj.Handle.ToString());
        }

        public static string ObjToTypeAndHandleStr(ObjectId objId)
        {
            string str;

            if (objId.IsNull)
                str = "(null)";
            else
            {
                // open up even if erased
                Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = objId.Database.TransactionManager;
                using (Autodesk.AutoCAD.DatabaseServices.Transaction tr = tm.StartTransaction())
                {
                    DBObject tmpObj = tr.GetObject(objId, OpenMode.ForRead, true);
                    str = ObjToTypeAndHandleStr(tmpObj);
                    tr.Commit();
                }
            }

            return str;
        }

        public static string ColorIndexToStr(int color, bool convertStandard)
        {
            string str;

            color = Math.Abs(color);    // in case used from Layer table DXF

            Debug.Assert((color >= 0) && (color <= 256));

            if (color == 0)
                str = "ByBlock";
            else if (color == 256)
                str = "ByLayer";
            else if (convertStandard)
            {
                if (color == 1)
                    str = "1-Red";
                else if (color == 2)
                    str = "2-Yellow";
                else if (color == 3)
                    str = "3-Green";
                else if (color == 4)
                    str = "4-Cyan";
                else if (color == 5)
                    str = "5-Blue";
                else if (color == 6)
                    str = "6-Magenta";
                else if (color == 7)
                    str = "7-White";
                else
                    str = color.ToString();
            }
            else
                str = color.ToString();

            return str;
        }

        public static string DbToStr(Database db)
        {
            if (db == null)
            {
                return "(null)";
            }

            return db.Filename;

            // TBD: what if this fails or returns NullStr?  ARX version used pointer value
            // to display
        }

        public static bool GetSelSetFromUser(out ObjectIdCollection selSet)
        {
            selSet = null;
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

            PromptSelectionResult res = ed.GetSelection();
            if (res.Status != PromptStatus.OK)
                return false;

            selSet = new ObjectIdCollection(res.Value.GetObjectIds());

            return true;
        }

        public static bool GetObject(out ObjectId objId)
        {
            objId = ObjectId.Null;
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

            PromptStringOptions prOpts = new PromptStringOptions(string.Format("\nHandle of Db Object"));
            prOpts.AllowSpaces = true;

            PromptResult prRes = ed.GetString(prOpts);

            if (prRes.Status != PromptStatus.OK)
                return false;

            var db = acadApp.DocumentManager.MdiActiveDocument.Database;
            //Handle handle = db.StringToHandle(prRes.StringResult);
            //objId = DbExtensions.HandleToObjectId(handle);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts3dCollection"></param>
        /// <returns></returns>
        public static bool GetPline3dCollection(Point3dCollection pts3dCollection)
        {
            if (pts3dCollection == null)
                return false;

            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            PromptPointOptions options = new PromptPointOptions("\nSelect Point");
            options.AllowNone = true;
            PromptPointResult ptResult = ed.GetPoint(options);

            /// Get the initial point
            Point3d basePt = new Point3d();
            if (ptResult.Status == PromptStatus.OK)
            {
                basePt = ptResult.Value;
            }

            /// loop till you break out
            while (true)
            {

                pts3dCollection.Add(basePt);
                int count = pts3dCollection.Count;
                if (count > 1)
                {
                    ///
                    /// NOTE: DrawVector wraps acedGrDraw, which provides a jig for free.
                    /// However here we have to set up our own pline jig
                    ///
                    ed.DrawVector(pts3dCollection[count - 2], pts3dCollection[count - 1], -1, false);
                    options.BasePoint = basePt;
                }

                /// drag with the help of a pline jig
                JigPline jig = new JigPline(basePt);
                PromptResult result = ed.Drag(jig);
                if (result.Status != PromptStatus.OK)
                {
                    break;
                }
                basePt = jig.EndPoint;
            }

            if (pts3dCollection.Count < 2)
            {
                PrintToCmdLine("There must be at least 2 points.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="corner1"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static bool GetCorners(out Point3d corner1, out Point3d corner2)
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

            PromptPointResult prRes = ed.GetPoint("\nSelect one corner");
            if (prRes.Status != PromptStatus.OK)
            {
                corner1 = prRes.Value;
                corner2 = prRes.Value;
                return false;
            }

            corner1 = prRes.Value;
            PromptCornerOptions prOpts = new PromptCornerOptions(string.Format("\n{0}", "Select another corner"), corner1);
            prOpts.AllowNone = true;
            prOpts.UseDashedLine = true;

            prRes = ed.GetCorner(prOpts);
            if (prRes.Status != PromptStatus.OK)
            {
                corner2 = prRes.Value;
                return false;
            }
            corner2 = prRes.Value;

            return true;
        }
    }
}
