using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Microsoft.CSharp;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Extensions
{

    public static class CommonExtensions
    {
        public static Extents3d GetViewExtents(this Extents3d extents)
        {
            // Gets the current view
            Document acDoc = acadApp.DocumentManager.MdiActiveDocument;
            ViewTableRecord acView = acDoc.Editor.GetCurrentView();

            // Translates WCS coordinates to DCS
            var matWcs2Dcs = Matrix3d.PlaneToWorld(acView.ViewDirection);
            matWcs2Dcs = Matrix3d.Displacement(acView.Target - Point3d.Origin) * matWcs2Dcs;
            matWcs2Dcs = Matrix3d.Rotation(-acView.ViewTwist, acView.ViewDirection, acView.Target) * matWcs2Dcs;

            // Tranforms the extents to DCS
            matWcs2Dcs = matWcs2Dcs.Inverse();
            extents.TransformBy(matWcs2Dcs);
            return extents;
        }

        public static Extents2d GetViewExtents(this Extents2d extents)
        {
            return GetViewExtents(extents.ToExtents3d()).ToExtents2d();
        }

        /// <summary>
        /// Creates a layout with the specified name and optionally makes it current.
        /// </summary>
        /// <param name="name">The name of the viewport.</param>
        /// <param name="select">Whether to select it.</param>
        /// <returns>The ObjectId of the newly created viewport.</returns>
        public static ObjectId CreateAndMakeLayoutCurrent(this LayoutManager lm, string name, bool select = true)
        {
            // First try to get the layout
            var id = lm.GetLayoutId(name);

            // If it doesn't exist, we create it
            if (!id.IsValid)
            {
                id = lm.CreateLayout(name);
            }

            // And finally we select it
            if (select)
            {
                lm.CurrentLayout = name;
            }

            return id;
        }

        /// <summary>
        /// Applies an action to the specified viewport from this layout.
        /// Creates a new viewport if none is found withthat number.
        /// </summary>
        /// <param name="tr">The transaction to use to open the viewports.</param>
        /// <param name="vpNum">The number of the target viewport.</param>
        /// <param name="f">The action to apply to each of the viewports.</param>
        public static void ApplyToViewport(this Layout lay, Transaction tr, int vpNum, Action<Viewport> f)
        {
            var vpIds = lay.GetViewports();
            Viewport vp = null;

            foreach (ObjectId vpId in vpIds)
            {
                var vp2 = tr.GetObject(vpId, OpenMode.ForWrite) as Viewport;
                if (vp2 != null && vp2.Number == vpNum)
                {
                    // We have found our viewport, so call the action
                    vp = vp2;
                    break;
                }
            }

            if (vp == null)
            {
                // We have not found our viewport, so create one
                var btr = (BlockTableRecord)tr.GetObject(lay.BlockTableRecordId, OpenMode.ForWrite);
                vp = new Viewport();

                // Add it to the database
                btr.AppendEntity(vp);
                tr.AddNewlyCreatedDBObject(vp, true);

                // Turn it - and its grid - on
                vp.On = true;
                vp.GridOn = true;
            }

            // Finally we call our function on it
            f(vp);
        }

        /// <summary>
        /// Apply plot settings to the provided layout.
        /// </summary>
        /// <param name="pageSize">The canonical media name for our page size.</param>
        /// <param name="styleSheet">The pen settings file (ctb or stb).</param>
        /// <param name="devices">The name of the output device.</param>

        /// <summary> Determine the maximum possible size for this layout. </summary>
        /// <returns>The maximum extents of the viewport on this layout.</returns>
        public static Extents2d GetMaximumExtents(this Layout lay)
        {
            // If the drawing template is imperial, we need to divide by
            // 1" in mm (25.4)

            var div = lay.PlotPaperUnits == PlotPaperUnit.Inches ? 25.4 : 1.0;

            // We need to flip the axes if the plot is rotated by 90 or 270 deg

            var doIt = lay.PlotRotation == PlotRotation.Degrees090 ||
                       lay.PlotRotation == PlotRotation.Degrees270;

            // Get the extents in the correct units and orientation
            var min = lay.PlotPaperMargins.MinPoint.Swap(doIt) / div;
            var max = (lay.PlotPaperSize.Swap(doIt) - lay.PlotPaperMargins.MaxPoint.Swap(doIt).GetAsVector()) / div;

            return new Extents2d(min, max);
        }

        /// <summary>
        /// Sets the size of the viewport according to the provided extents.
        /// </summary>
        /// <param name="ext">The extents of the viewport on the page.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void ResizeViewport(this Viewport vp, Extents2d ext, double fac = 1.0)
        {
            vp.Width = (ext.MaxPoint.X - ext.MinPoint.X) * fac;
            vp.Height = (ext.MaxPoint.Y - ext.MinPoint.Y) * fac;
            vp.CenterPoint = (Point2d.Origin + (ext.MaxPoint - ext.MinPoint) * 0.5).Pad();
        }

        /// <summary>
        /// Sets the view in a viewport to contain the specified model extents.
        /// </summary>
        /// <param name="ext">The extents of the content to fit the viewport.</param>
        /// <param name="fac">Optional factor to provide padding.</param>
        public static void FitContentToViewport(this Viewport vp, Extents3d ext, double fac = 1.0)
        {
            // Let's zoom to just larger than the extents
            vp.ViewCenter = (ext.MinPoint + ((ext.MaxPoint - ext.MinPoint) * 0.5)).ToPoint2d();

            // Get the dimensions of our view from the database extents
            var hgt = ext.MaxPoint.Y - ext.MinPoint.Y;
            var wid = ext.MaxPoint.X - ext.MinPoint.X;

            // We'll compare with the aspect ratio of the viewport itself
            // (which is derived from the page size)
            var aspect = vp.Width / vp.Height;

            // If our content is wider than the aspect ratio, make sure we
            // set the proposed height to be larger to accommodate the
            // content
            if (wid / hgt > aspect)
            {
                hgt = wid / aspect;
            }

            // Set the height so we're exactly at the extents
            vp.ViewHeight = hgt;

            // Set a custom scale to zoom out slightly (could also
            // vp.ViewHeight *= 1.1, for instance)
            vp.CustomScale *= fac;
        }

        public static bool FindMatch(this string input, string pattern)
        {
            if (string.IsNullOrEmpty(input.Trim())) return false;

            //var match = Regex.Match(input, $"^{pattern}$", RegexOptions.IgnoreCase);
            //bool result = match.Success;
            var isMatch = Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
            return isMatch;
        }

        public static bool FindMatches(this string input, string[] patterns)
        {
            bool result = false;
            foreach (string pattern in patterns)
            {
                var match = Regex.Match(input, $"^{pattern}$", RegexOptions.IgnoreCase);
                result = match.Success;
            }
            return result;
            //return Regex.Matches(string.Join(";", strArray), @"\b[A-Za-z-']+\b")
            //    .OfType<Match>()
            //    .Select(m => m.Groups[0].Value);
        }
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool IsSearchPatternValid(this string pattern, Func<bool> condintion = null)
        {
            if (pattern == null) return true;

            bool isPartSearch = !string.IsNullOrEmpty(pattern.Replace("*", ""));
            bool isCondintion = true;

            if (condintion != null && isPartSearch)
                isCondintion = condintion();

            return pattern == "*" || isPartSearch && isCondintion;
        }

        public static string[] PatternNormalize(this string[] items)
        {
            if (items == null || items.Any(x => x == "*"))
                items = new[] { ".*" };
            else if (items.Any(x => x.Contains("*") && !x.Contains(".*")))
                items = items
                    .Where(x => x.Contains("*") && !x.Contains(".*"))
                    .Select(x => x.Replace("*", ".*")).ToArray();
            return items;
        }

        public static IList<T> Clone<T>(this IEnumerable<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static Expression<TDelegate> AndAlso<TDelegate>(this Expression<TDelegate> left, Expression<TDelegate> right)
        {
            return System.Linq.Expressions.Expression.Lambda<TDelegate>(System.Linq.Expressions.Expression.AndAlso(left, right), left.Parameters);
        }

        /// <summary> Get current Class name + Method name </summary>
        public static string GetFullNameBundle(this MethodBase methodBase)
        {
            return methodBase.ReflectedType.Name + "." + methodBase.Name;
        }

        /// <summary> XDisplay </summary>
        public static void XDisplay(this Stopwatch sw)
        {
            //Log.Add("Watch", "Time elapsed: {0} min.\n", sw.Elapsed.TotalMinutes);
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
            { throw new InvalidExpressionException(String.Format("Error ({0}) evaluating: {1}", cr.Errors[0].ErrorText, sExpression)); }

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

        public static string GetSortPropertyName(DependencyObject element, DependencyProperty attachPropertyName)
        {
            if (element == null) return string.Empty;
            return (string)element.GetValue(attachPropertyName);
        }


        public static T Clone<T>(this T obj)
        {
            var inst = obj.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            return (T)inst?.Invoke(obj, null);
        }

        public static T GetAssemblyAttribute<T>(this Assembly assembly) where T : Attribute
        {
            if (assembly == null) return null;
            object[] attributes = assembly.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 0) return null;
            return (T)attributes[0];
        }

        //T XToDictionary1<T1, T>(this IEnumerable<T> tList, object obj)
        //{
        //    return (Dictionary<T1, T>)tList.ToDictionary(p => p.ToString());
        //} 

        // Writes an Excel file from the datatable (using late binding)
        //public static void WriteXls(this DataTable dataTbl, string filename, string sheetName, bool visible)
        //{
        //    object mis = Type.Missing;
        //    object xlApp = LateBinding.GetOrCreateInstance("Excel.Application");
        //    xlApp.Set("DisplayAlerts", false);
        //    object workbooks = xlApp.Get("Workbooks");
        //    object workbook, worksheet;
        //    if (File.Exists(filename))
        //        workbook = workbooks.Invoke("Open", filename);
        //    else
        //        workbook = workbooks.Invoke("Add", mis);
        //    if (String.IsNullOrEmpty(sheetName))
        //        worksheet = workbook.Get("Activesheet");
        //    else
        //    {
        //        object worksheets = workbook.Get("Worksheets");
        //        try
        //        {
        //            worksheet = worksheets.Get("Item", sheetName);
        //            worksheet.Get("Cells").Invoke("Clear");
        //        }
        //        catch
        //        {
        //            worksheet = worksheets.Invoke("Add", mis);
        //            worksheet.Set("Name", sheetName);
        //        }
        //    }
        //    object range = worksheet.Get("Cells");
        //    dataTbl.GetColumnNames()
        //        .Iterate((name, i) => range.Get("Item", 1, i + 1).Set("Value2", name));
        //    dataTbl.Rows
        //        .Cast<DataRow>()
        //        .Iterate((row, i) => row.ItemArray
        //            .Iterate((item, j) => range.Get("Item", i + 2, j + 1).Set("Value2", item)));
        //    xlApp.Set("DisplayAlerts", true);
        //    if (visible)
        //    {
        //        xlApp.Set("Visible", true);
        //    }
        //    else
        //    {
        //        if (File.Exists(filename))
        //            workbook.Invoke("Save");
        //        else
        //        {
        //            int fileFormat =
        //                String.Compare("11.0", (string)xlApp.Get("Version")) < 0 &&
        //                filename.EndsWith(".xlsx", StringComparison.CurrentCultureIgnoreCase) ?
        //                    51 : -4143;
        //            workbook.Invoke("Saveas", filename, fileFormat, String.Empty, String.Empty, false, false, 1, 1);
        //        }
        //        workbook.Invoke("Close");
        //        workbook = null;
        //        xlApp.ReleaseInstance();
        //        xlApp = null;
        //    }
        //}
        public static void XWriteCsv(this List<string> allLines, string filePath)
        {
            var csv = new StringBuilder();
            allLines.ForEach(line =>
            {
                csv.AppendLine(string.Join(",", line));
            });
            File.WriteAllText(filePath, csv.ToString());
        }

        // Writes a csv file from the datatable.
        public static void WriteCsv2(this System.Data.DataTable dataTbl, string filename)
        {
            //using (StreamWriter writer = new StreamWriter(filename))
            //{
            //    writer.WriteLine(dataTbl.GetColumnNames().Aggregate((s1, s2) => String.Format("{0},{1}", s1, s2)));
            //    dataTbl.Rows
            //        .Cast<DataRow>()
            //        .Select(row => row.ItemArray.Aggregate((s1, s2) => String.Format("{0},{1}", s1, s2)))
            //        .Iterate(line => writer.WriteLine(line));
            //}
        }

        // Creates an AutoCAD Table from the datatable.
        //public static Autodesk.AutoCAD.DatabaseServices.Table ToAcadTable(this System.Data.DataTable dataTbl, double rowHeight, double columnWidth)
        //{
        //    //return dataTbl.Rows.Cast<DataRow>().ToAcadTable(dataTbl.TableName, dataTbl.GetColumnNames(), rowHeight, columnWidth);
        //    Autodesk.AutoCAD.DatabaseServices.Table tbl = new Autodesk.AutoCAD.DatabaseServices.Table();
        //    //tbl.Rows[0].Height = rowHeight;
        //    //tbl.Columns[0].Width = columnWidth;
        //    //tbl.InsertColumns(0, columnWidth, dataTbl.Columns.Count - 1);
        //    //tbl.InsertRows(0, rowHeight, dataTbl.Rows.Count + 1);
        //    //tbl.Cells[0, 0].Value = dataTbl.TableName;
        //    //dataTbl.GetColumnNames()
        //    //    .Iterate((name, i) => tbl.Cells[1, i].Value = name);
        //    //dataTbl.Rows
        //    //    .Cast<DataRow>()
        //    //    .Iterate((row, i) =>
        //    //        row.ItemArray.Iterate((item, j) =>
        //    //            tbl.Cells[i + 2, j].Value = item));
        //    return tbl;
        //}
    }
}
