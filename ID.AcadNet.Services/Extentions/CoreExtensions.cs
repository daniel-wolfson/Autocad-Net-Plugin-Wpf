using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure;
using Intellidesk.AcadNet.Common.Extensions;
using Microsoft.CSharp;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Db = Autodesk.AutoCAD.DatabaseServices;
using Ed = Autodesk.AutoCAD.EditorInput;
using Rt = Autodesk.AutoCAD.Runtime;

namespace Intellidesk.AcadNet.Services.Extentions
{
    /// <summary> General </summary>
    public static class CoreExtensions
    {
        public static ResultBuffer StreamToResBuf(this MemoryStream ms, string appName)
        {
            int kMaxChunkSize = 127;

            ResultBuffer resBuf = new ResultBuffer(new TypedValue(
               (int)DxfCode.ExtendedDataRegAppName, appName));

            for (int i = 0; i < ms.Length; i += kMaxChunkSize)
            {
                int length = (int)Math.Min(ms.Length - i, kMaxChunkSize);
                byte[] datachunk = new byte[length];
                ms.Read(datachunk, 0, length);
                resBuf.Add(new TypedValue((int)DxfCode.ExtendedDataBinaryChunk, datachunk));
            }

            return resBuf;
        }

        public static MemoryStream ResBufToStream(this ResultBuffer resBuf)
        {
            MemoryStream ms = new MemoryStream();
            TypedValue[] values = resBuf.AsArray();

            // Start from 1 to skip application name

            for (int i = 1; i < values.Length; i++)
            {
                byte[] datachunk = (byte[])values[i].Value;
                ms.Write(datachunk, 0, datachunk.Length);
            }
            ms.Position = 0;

            return ms;
        }

        public static ResultBuffer SaveToResBuf(this object that, string appName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, that);
            ms.Position = 0;

            ResultBuffer resBuf = ms.StreamToResBuf(appName);
            return resBuf;
        }

        public static object Convert(object value)
        {
            if (value == null)
                return new TypedValue((int)Rt.LispDataType.Nil);
            if (value is IEnumerable)
            {
                ResultBuffer rb = new ResultBuffer();
                AddToRB(rb, value);
                return rb;
            }
            return Convert(value as string);
        }


        #region Private helper functions for enumerable types
        // Unhandled objects in enumerables
        private static void AddToRB(ResultBuffer rb, object value)
        {
            rb.Add(Convert(value));
        }

        // Tuples to dotted pairs
        private static void AddToRB(ResultBuffer rb, Tuple<object, object> value)
        {
            rb.Add(new TypedValue((int)Rt.LispDataType.DottedPair));
            AddToRB(rb, value.Item1);
            AddToRB(rb, value.Item2);
            rb.Add(new TypedValue((int)Rt.LispDataType.ListEnd));
        }

        // Dictionary entry to association pair
        private static void AddToRB(ResultBuffer rb, DictionaryEntry value)
        {
            IEnumerable en = value.Value as IEnumerable;
            if (en == null)
            {
                rb.Add(new TypedValue((int)Rt.LispDataType.DottedPair));
                AddToRB(rb, value.Key);
                AddToRB(rb, value.Value);
            }
            else
            {
                rb.Add(new TypedValue((int)Rt.LispDataType.ListBegin));
                rb.Add(Convert(value.Key));
                foreach (object element in en)
                {
                    AddToRB(rb, element);
                }
            }
            rb.Add(new TypedValue((int)Rt.LispDataType.ListEnd));
        }

        // Hash tables to association lists
        private static void AddToRB(ResultBuffer rb, IDictionary value)
        {
            rb.Add(new TypedValue((int)Rt.LispDataType.ListBegin));
            IDictionaryEnumerator en = value.GetEnumerator();
            while (en.MoveNext())
            {
                AddToRB(rb, en.Current);
            }
            rb.Add(new TypedValue((int)Rt.LispDataType.ListEnd));
        }

        // Lists, collections and arrays to Lisp Lists
        private static void AddToRB(ResultBuffer rb, IEnumerable value)
        {
            rb.Add(new TypedValue((int)Rt.LispDataType.ListBegin));
            foreach (object element in value)
            {
                AddToRB(rb, element);
            }
            rb.Add(new TypedValue((int)Rt.LispDataType.ListEnd));
        }
        #endregion
    }

    public static class CoreExtensionsTemp
    {
        public static Editor Ed { get { return Doc.Editor; } }
        public static Database Db { get { return Doc.Database; } }
        public static Document Doc { get { return acadApp.DocumentManager.MdiActiveDocument; } }


        /// <summary> Common Color Dialog </summary>
        public static Color DlgColor
        {
            get
            {
                var cd = new ColorDialog { IncludeByBlockByLayer = true };
                cd.ShowModal();
                return cd.Color;
            }
        }

        //The current LineType
        private static ObjectId _lineTypeDefaultId;

        /// <summary> LineType </summary>
        public static ObjectId LineType(string value)
        {
            var retValue = Db.Celtype;
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                var lt = (LinetypeTable)tr.GetObject(Db.LinetypeTableId, OpenMode.ForRead);
                //Dim acLineTypTbl As LinetypeTable = WorkSpace.Db.LinetypeTableId.Open(OpenMode.ForRead)
                if (lt != null)
                    if (lt.Has(value) == false)
                    {
                        Plugin.Logger.Warning($"{nameof(CoreExtensions)}.{nameof(LineType)}", "Warning", "LineType " + value + " not found. Will be used Solid type");
                    }
                    else
                    {
                        retValue = lt[value];
                    }
                tr.Commit();
                return retValue;
            }
        }

        /// <summary> LineType </summary>
        public static ObjectId LineType(int value = 0)
        {
            using (var tr = Db.TransactionManager.StartTransaction())
            {
                LinetypeTable lt;
                string ltNameDefault = "Continuous";
                switch (value)
                {
                    case -2:
                        //Load All Linetypes
                        using (Doc.LockDocument())
                        {
                            string path = HostApplicationServices.Current.FindFile("acad.lin", Db, FindFileHint.Default);
                            lt = tr.GetObject(Db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable; //LinetypeTable lt = WorkSpace.Db.LinetypeTableId.Open(OpenMode.ForRead)
                            if (lt != null && lt.Has("DASHED") == false)
                            {
                                Db.LoadLineTypeFile("DASHED", path);
                            }
                            //Db.Celtype = lt(ltNameDefault)  '' Set the current linetype
                        }

                        break;
                    case -1:
                        //Dialog mode
                        var ltDlg = new LinetypeDialog { IncludeByBlockByLayer = true };
                        ltDlg.ShowModal();
                        var ltr = tr.GetObject(ltDlg.Linetype, OpenMode.ForRead) as LinetypeTableRecord;
                        if (ltr != null) ltNameDefault = ltr.Name;
                        _lineTypeDefaultId = ltDlg.Linetype;
                        break;
                    case 0:
                        ltNameDefault = "Continuous";
                        break;
                    case 1:
                        ltNameDefault = "DASHED";
                        break;
                    case 2:
                        ltNameDefault = "Center";
                        break;
                    case 3:
                        ltNameDefault = "DOT";
                        break;
                    default:
                        ltNameDefault = "Dashed";
                        break;
                }
                lt = tr.GetObject(Db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                //Dim acLineTypTbl As LinetypeTable = WorkSpace.Db.LinetypeTableId.Open(OpenMode.ForRead)
                if (lt != null && lt.Has(ltNameDefault) == false)
                {
                    Ed.WriteMessage("LineType " + ltNameDefault + " not found");
                    ltNameDefault = "Continuous";
                }
                tr.Commit();
                if (lt != null) _lineTypeDefaultId = lt[ltNameDefault];
            }
            return _lineTypeDefaultId;
        }

        /// <summary> XDisplay </summary>
        public static void XDisplay(this Stopwatch sw)
        {
            Plugin.Logger.Error("Watch,Time elapsed: {0} min.\n", sw.Elapsed.TotalMinutes);
        }

        /// <summary> GetAcadVersion </summary>
        public static string GetAcadVersion()
        {
            var res = "";
            var regKey = Registry.CurrentUser.OpenSubKey("Software\\\\Autodesk\\\\AutoCAD", false);
            if (regKey != null)
            {
                res = regKey.GetValue("CurVer", "").ToString().Substring(0, regKey.GetValue("CurVer", "").ToString().Length - 1);
                regKey.Close();
            }
            return res;
        }

        private static object Eval(string sExpression)
        {
            //object result = Eval("1 + 3"); 
            //string now    = Eval("System.DateTime.Now().ToString()") as string
            var c = new CSharpCodeProvider();
            var cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("system.dll");
            cp.CompilerOptions = "/t:library";
            cp.GenerateInMemory = true;
            var sb = new StringBuilder("");
            sb.Append("using System;\n");
            sb.Append("namespace CSCodeEvaler{ \n");
            sb.Append("public class CSCodeEvaler{ \n");
            sb.Append("public object EvalCode(){\n");
            sb.Append("return " + sExpression + "; \n");
            sb.Append("} \n");
            sb.Append("} \n");
            sb.Append("}\n");

            var cr = c.CompileAssemblyFromSource(cp, sb.ToString());
            if (cr.Errors.Count > 0)
            { throw new InvalidExpressionException(string.Format("Error ({0}) evaluating: {1}", cr.Errors[0].ErrorText, sExpression)); }

            var a = cr.CompiledAssembly; object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");
            var t = o.GetType();
            var mi = t.GetMethod("EvalCode");
            var s = mi.Invoke(o, null);
            return s;
        }

        /// <summary> XConvertType </summary>
        public static T XConvertType<T1, T>(this T1 tObject)
        {
            return (T)Convert.ChangeType(tObject, typeof(T));
        }

        /// <summary> To make that changes appear on the screen </summary>
        public static void XTransFlush(this Transaction tr)
        {
            tr.TransactionManager.QueueForGraphicsFlush();
            acadApp.DocumentManager.MdiActiveDocument.Editor.Regen();
        }

        /// <summary> CreateComplexLinetype </summary>
        public static void CreateComplexLinetype()
        {
            var tr = Db.TransactionManager.StartTransaction();
            using (tr)
            {
                // We'll use the textstyle table to access
                // the "Standard" textstyle for our text
                // segment

                var tt = (TextStyleTable)tr.GetObject(Db.TextStyleTableId, OpenMode.ForRead);

                // Get the linetype table from the drawing
                var lt = (LinetypeTable)tr.GetObject(Db.LinetypeTableId, OpenMode.ForWrite);

                // Create our new linetype table record...
                var ltr = new LinetypeTableRecord // ... and set its properties
                {
                    Name = "COLD_WATER_SUPPLY",
                    AsciiDescription = "Cold water supply ---- CW ---- CW ---- CW ----",
                    PatternLength = 0.9,
                    NumDashes = 3
                };
                // Dash #1
                ltr.SetDashLengthAt(0, 0.5);

                // Dash #2
                ltr.SetDashLengthAt(1, -0.2);
                ltr.SetShapeStyleAt(1, tt["Standard"]);
                ltr.SetShapeNumberAt(1, 0);
                ltr.SetShapeOffsetAt(1, new Vector2d(-0.1, -0.05));
                ltr.SetShapeScaleAt(1, 0.1);
                ltr.SetShapeIsUcsOrientedAt(1, false);
                ltr.SetShapeRotationAt(1, 0);
                ltr.SetTextAt(1, "CW");

                // Dash #3
                ltr.SetDashLengthAt(2, -0.2);

                // Add the new linetype to the linetype table
                ObjectId ltId = lt.Add(ltr);

                tr.AddNewlyCreatedDBObject(ltr, true);

                // Create a test line with this linetype
                var bt = (BlockTable)tr.GetObject(Db.BlockTableId, OpenMode.ForRead);
                var btr = (BlockTableRecord)tr.GetObject(bt[acadApp.DocumentManager.GetCurrentSpace()], OpenMode.ForWrite);

                var ln = new Line(new Point3d(0, 0, 0), new Point3d(10, 10, 0));

                ln.SetDatabaseDefaults(Db);
                ln.LinetypeId = ltId;

                btr.AppendEntity(ln);
                tr.AddNewlyCreatedDBObject(ln, true);

                tr.Commit();
            }

        }
    }
}