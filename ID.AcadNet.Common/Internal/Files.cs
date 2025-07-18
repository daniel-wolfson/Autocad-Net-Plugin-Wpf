using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using acApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Intellidesk.AcadNet.Common.Internal
{
    public static class Files
    {
        private static IPluginSettings PluginSettings => Plugin.GetService<IPluginSettings>();
        private static List<ObjectId> CurrentObjects = new List<ObjectId>();

        #region "External operations: Xml, Dwg, Db"
        //LoadDataFromDwg
        public static bool LoadDataFromDwg(string tBlockName = "")
        {
            var db = HostApplicationServices.WorkingDatabase;
            var doc = acApp.DocumentManager.MdiActiveDocument;
            var blockRefs = new List<ObjectId>();

            try
            {
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    var pntXcoll = new List<double>();
                    var pntYcoll = new List<double>();
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                    if (bt.Has(tBlockName))
                    {
                        var btr = (BlockTableRecord)bt[tBlockName].GetObject(OpenMode.ForRead);
                        blockRefs.Clear();
                        foreach (ObjectId objId in btr.GetBlockReferenceIds(true, true))
                        {
                            blockRefs.Add(objId);

                            var br = (BlockReference)tr.GetObject(objId, OpenMode.ForRead);

                            if (btr.HasAttributeDefinitions) // & FlgAttrRead
                            {
                                //AttributeReference
                                foreach (ObjectId arId in br.AttributeCollection)
                                {
                                    var obj = tr.GetObject(arId, OpenMode.ForRead);
                                    var ar = (AttributeReference)obj;
                                    //Add(ar.Tag.ToString().Trim(), ar.TextString.ToString().Trim(), obj.ObjectId);
                                }
                                //FlgAttrRead = false;
                            }
                            pntXcoll.Add(br.Position.X);
                            pntYcoll.Add(br.Position.Y);
                        }

                        //isBlockFound = True
                    }
                    else
                    {
                        doc.Editor.WriteMessage("LoadDataFromDwg : Block not found! Loaded Data default");
                        //LoadDataDefault();
                        //isBlockFound = False
                        return false;
                    }
                    tr.Commit();
                }

            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Plugin.Logger.Error($"{nameof(Files)}.{nameof(LoadDataFromDwg)} error: ", ex);
            }
            return false;
        }

        //LoadDataFromDwg
        public static void ReadDataFromXml(string tName = "")
        {
            //Throw New NotImplementedException("Xml Data not readed")
            try
            {
                //LoadDataDefault();
                var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().FullName);
                var mySettings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true };
                using (var reader = XmlReader.Create(string.IsNullOrEmpty(tName) ? rootPath + "UtilDat.xml" : tName, mySettings))
                {
                    reader.MoveToContent();
                    reader.Read();
                    //for (int fieldId = 0; fieldId <= TagNames.Count - 1; fieldId++)
                    //{
                    //    if (reader.LocalName == TagNames(fieldId))
                    //    {
                    //        TagValues(fieldId) = reader.ReadString();
                    //    }
                    //    reader.Read();
                    //}
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public static object LoadDataFromMdb(string tName = "", string tFilter = "", string tSort = "")
        {
            try
            {
                //const string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + "RootPath" + "QW.MDB";
                //using (var connection = new System.Data.OleDb.OleDbConnection(connectionString))
                //{
                //    var command = new System.Data.OleDb.OleDbCommand("Select * from Table1" + (string.IsNullOrEmpty(tFilter) ? "" : " Where " + tFilter) + (string.IsNullOrEmpty(tSort) ? "" : " By " + tSort), connection);
                //    connection.Open();
                //    var reader = command.ExecuteReader();
                //    while (reader != null && reader.Read())
                //    {
                //        Console.WriteLine(reader[0].ToString());
                //    }
                //    if (reader != null) reader.Close();
                //}
                return true;

            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(Files)}.{nameof(LoadDataFromMdb)} error: ", ex);
                MessageBox.Show(ex.Message);
                return false;
            }
        } //LoadDataFromMdb
          // Write Data from DataSet To Xml file "UtilDat.xml"

        public static void WriteDataToXml(string tName = "")
        {
            try
            {
                IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
                var rootPath = pluginSettings.RootPath;//@"C:\Users\dimitryx\My Projects\X\Projects\"
                var settings = new XmlWriterSettings { Indent = true, IndentChars = "    " };
                using (var writer = XmlWriter.Create(string.IsNullOrEmpty(tName) ? rootPath + "MYIL.xml" : tName, settings))
                {
                    var fileName = Path.GetFileName("Block_Name_U5");
                    if (fileName != "")
                        writer.WriteStartElement(fileName.Replace(" ", "_"));
                    for (var fieldId = 0; fieldId <= CurrentObjects.Count - 1; fieldId++)
                    {
                        var extents3D = ((Entity)CurrentObjects[fieldId].GetObject(OpenMode.ForRead)).GeometricExtents;
                        writer.WriteElementString("Xmin", extents3D.MinPoint.X.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("Ymin", extents3D.MinPoint.Y.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("Xmax", extents3D.MaxPoint.X.ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("Ymax", extents3D.MaxPoint.Y.ToString(CultureInfo.InvariantCulture));
                    }
                    writer.WriteEndElement();
                    writer.Flush();
                }

            }
            catch (Exception ex)
            {
                Plugin.Logger.Error($"{nameof(Files)}.{nameof(WriteDataToXml)} error: ", ex);
            }
        }

        public static void LoadDataFromXml(string fileName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load("test-Doc.xml");

            //string nodepath = "/SubmissionTypes/SubmissionType[@typename='" + SubmissionType + "']";
            //XmlNode node = xmlDoc.SelectSingleNode(nodepath); 

            XmlNodeList userNodes = xmlDoc.SelectNodes("//users/user");
            if (userNodes != null)
                foreach (XmlNode userNode in userNodes)
                {
                    if (userNode.Attributes != null)
                    {
                        var age = int.Parse(userNode.Attributes["Xmin"].Value);
                        userNode.Attributes["Xmin"].Value = Convert.ToString(age + 1);
                    }
                }
            xmlDoc.Save("test-Doc.xml");
        }

        #endregion

        public static string GetValueValid(string tVal, string tValDefault)
        {
            //    'Throw New NotImplementedException
            //    Dim dblVal As Double = Nothing
            //    Try
            //        dblVal = CType(tVal, Double)
            //        tVal = tVal
            //    Catch ex As InvalidCastException
            //        tVal = tValDefault
            //    End Try
            return null;
        }
        public static void ScreenShotToFile(Autodesk.AutoCAD.Windows.Window wd, string filename, int top = 0, int bottom = 0, int left = 0, int right = 0)
        {
            Point pt = new Point((int)wd.DeviceIndependentLocation.X, (int)wd.DeviceIndependentLocation.Y);
            Size sz = new Size((int)wd.DeviceIndependentSize.Width, (int)wd.DeviceIndependentSize.Height);

            pt.X += left;
            pt.Y += top;
            sz.Height -= top + bottom;
            sz.Width -= left + right;

            // Set the bitmap object to the size of the screen
            Bitmap bmp = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppArgb); //.Format32bppArgb
            using (bmp)
            {
                // Create a graphics object from the bitmap
                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    // Take a screenshot of our window
                    gfx.CopyFromScreen(pt.X, pt.Y, 0, 0, sz, CopyPixelOperation.SourceCopy);

                    // Save the screenshot to the specified location
                    bmp.Save(filename, ImageFormat.Png);
                }
            }
        }

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point pt);

        // Command to capture a user-selected portion of a drawing
        [CommandMethod("WSS")]
        public static void WindowScreenShot()
        {
            Document doc = acApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Ask the user for the screen window to capture

            PromptPointResult ppr = ed.GetPoint("\nSelect first point of capture window: ");
            if (ppr.Status != PromptStatus.OK)
                return;
            Point3d first = ppr.Value;

            ppr = ed.GetCorner("\nSelect second point of capture window: ", first);
            if (ppr.Status != PromptStatus.OK)
                return;
            Point3d second = ppr.Value;

            // Generate screen coordinate points based on the
            // drawing points selected
            Point pt1, pt2;

            // First we get the viewport number
            short vp = (short)acApp.GetSystemVariable("CVPORT");

            // Then the handle to the current drawing window
            IntPtr hWnd = doc.Window.Handle;

            // Now calculate the selected corners in screen coordinates
            pt1 = ScreenFromDrawingPoint(ed, hWnd, first, vp);
            pt2 = ScreenFromDrawingPoint(ed, hWnd, second, vp);

            // Now save this portion of our screen as a raster image
            ScreenShotToFile(pt1, pt2, null, true);
        }

        // Perform our three tranformations to get from UCS (or WCS)
        // to screen coordinates

        public static Point ScreenFromDrawingPoint(Editor ed, IntPtr hWnd, Point3d ucsPt, short vpNum)
        {
            Point3d wcsPt = ucsPt.TransformBy(ed.CurrentUserCoordinateSystem);
            var pnt = ed.PointToScreen(wcsPt, vpNum);
            Point res = new Point((int)pnt.X, (int)pnt.Y);
            ClientToScreen(hWnd, ref res);
            return res;
        }

        // Save the display of an AutoCAD window as a raster file
        // and/or an image on the clipboard

        public static void ScreenShotToFile(Autodesk.AutoCAD.Windows.Window wd, int top, int bottom, int left, int right, string filename, bool clipboard)
        {
            Point pt = new Point((int)wd.DeviceIndependentLocation.X, (int)wd.DeviceIndependentLocation.Y);
            Size sz = new Size((int)wd.DeviceIndependentSize.Width, (int)wd.DeviceIndependentSize.Height);

            pt.X += left;
            pt.Y += top;
            sz.Height -= top + bottom;
            sz.Width -= left + right;

            SaveScreenPortion(pt, sz, filename, clipboard);
        }

        // Save a screen window between two corners as a raster file and/or an image on the clipboard
        public static void ScreenShotToFile(Point pt1, Point pt2, string filename, bool clipboard)
        {
            // Create the top left corner from the two corners
            // provided (by taking the min of both X and Y values)

            Point pt = new Point(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y));

            // Determine the size by subtracting X & Y values and
            // taking the absolute value of each

            Size sz = new Size(Math.Abs(pt1.X - pt2.X), Math.Abs(pt1.Y - pt2.Y));
            SaveScreenPortion(pt, sz, filename, clipboard);
        }

        // Save a portion of the screen display as a raster file and/or an image on the clipboard
        public static void SaveScreenPortion(Point pt, Size sz, string filename, bool clipboard)
        {
            // Set the bitmap object to the size of the window

            Bitmap bmp = new Bitmap(sz.Width, sz.Height, PixelFormat.Format32bppArgb);
            using (bmp)
            {
                // Create a graphics object from the bitmap

                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    // Take a screenshot of our window
                    gfx.CopyFromScreen(pt.X, pt.Y, 0, 0, sz, CopyPixelOperation.SourceCopy);

                    // Save the screenshot to the specified location
                    if (filename != null && filename != "")
                        bmp.Save(filename, ImageFormat.Png);

                    // Copy it to the clipboard
                    if (clipboard)
                        System.Windows.Forms.Clipboard.SetImage(bmp);
                }
            }
        }

        public static Bitmap FetchBitmapFromDwgFile(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    fs.Seek(0xD, SeekOrigin.Begin);
                    fs.Seek(0x14 + br.ReadInt32(), SeekOrigin.Begin);
                    byte bytCnt = br.ReadByte();
                    if (bytCnt <= 1)
                        return null;
                    int imageHeaderStart;
                    int imageHeaderSize;
                    byte imageCode;
                    for (short i = 1; i <= bytCnt; i++)
                    {
                        imageCode = br.ReadByte();
                        imageHeaderStart = br.ReadInt32();
                        imageHeaderSize = br.ReadInt32();
                        if (imageCode == 2)
                        {
                            fs.Seek(imageHeaderStart, SeekOrigin.Begin);
                            //-----------------------------------------------------
                            // BITMAPINFOHEADER (40 bytes)
                            br.ReadBytes(0xE); //biSize, biWidth, biHeight, biPlanes
                            ushort biBitCount = br.ReadUInt16();
                            br.ReadBytes(4); //biCompression
                            uint biSizeImage = br.ReadUInt32();
                            //br.ReadBytes(0x10); //biXPelsPerMeter, biYPelsPerMeter, biClrUsed, biClrImportant
                            //-----------------------------------------------------
                            fs.Seek(imageHeaderStart, SeekOrigin.Begin);
                            byte[] bitmapBuffer = br.ReadBytes(imageHeaderSize);
                            uint colorTableSize = (uint)(biBitCount < 9 ? 4 * Math.Pow(2, biBitCount) : 0);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                using (BinaryWriter bw = new BinaryWriter(ms))
                                {
                                    //bw.Write(new byte[] { 0x42, 0x4D });
                                    bw.Write((ushort)0x4D42);
                                    bw.Write(54U + colorTableSize + biSizeImage);
                                    bw.Write(new ushort());
                                    bw.Write(new ushort());
                                    bw.Write(54U + colorTableSize);
                                    bw.Write(bitmapBuffer);
                                    return new Bitmap(ms);
                                }
                            }
                        }
                        else if (imageCode == 3)
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }


    }
}

