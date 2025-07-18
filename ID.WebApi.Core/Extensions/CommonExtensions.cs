using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace General.Infrastructure.WebApi.Core.Extensions
{
    public enum eGeoCommands
    {
        LineString, MultiLineString, Point, MultiPoint, Poligon, MultiPoligon
    }
    public static class CommonExtensions
    {
        public static void XSaveDataAsEsriGeoJson(this List<ObjectId> ids)
        {
            Entity ent;
            GeoJSONObject geometry;
            Feature feature = null;
            FeatureCollection featureCollection = new FeatureCollection();
            string fileName = "export_esri_data";
            try
            {
                ids.ForEach(id =>
                {
                    ent = id.XGetEntity();
                    if (ent == null)
                        return;

                    var properies = new Dictionary<string, object> { { "color", ent.Color.ToHtmlColor() } };

                    if (ent.GetType().BaseType == typeof(Curve))
                    {
                        fileName = "lines_data.geojson";
                        var points = ent.XGetPoints();
                        List<IPosition> positions = new List<IPosition>();
                        foreach (Point3d pnt in points)
                        {
                            double lat, lng;
                            Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);
                            if (positions.Count() <= 2 || !positions.Any(x => x.Latitude == lat && x.Longitude == lng))
                                positions.Add(new Position(lat, lng, null));
                        }
                        geometry = new LineString(positions);
                        properies.Add("EntityType", ent.GetType().Name);

                        Type type = ent.XGetXDataObjectType();
                        if (type != null)
                        {
                            IElementDefinition element = ent.XGetXDataObject();
                            if (element != null)
                                properies.Add("entityName", element.Name);
                        }

                        feature = new Feature((LineString)geometry, properies, null);
                    }
                    else if (ent.GetType() == typeof(BlockReference))
                    {
                        fileName = "markers_data.geojson";
                        Point3d pnt = ent.XGetBasePoint();
                        double lat, lng;
                        Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);
                        var point = new Position(lat, lng, null);
                        geometry = new Point(new Position(point.Latitude, point.Longitude)); //mapinfo need for change: Longitude <--> Latitude
                        properies.Add("entityType", "BlockReference");

                        var br = (BlockReference)ent;
                        properies.Add("entityName", br.Name);

                        if (br.AttributeCollection != null)
                        {
                            using (var scope = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.TransactionScope())
                            {
                                var columns = new Dictionary<string, string>();
                                foreach (ObjectId attrId in br.AttributeCollection)
                                {
                                    AttributeReference attr = (AttributeReference)scope.tr.GetObject(attrId, OpenMode.ForRead);
                                    if (!columns.Keys.Contains(attr.Tag.ToLower()))
                                        columns.Add(attr.Tag.ToLower(), attr.TextString);
                                }
                                properies.Add("entityValue", JsonConvert.SerializeObject(columns));
                            }
                        }

                        feature = new Feature((Point)geometry, properies, null);
                    }
                    else if (ent.GetType() == typeof(DBText))
                    {
                        fileName = "text_data.json";
                        Point3d pnt = ent.XGetBasePoint();
                        double lat, lng;
                        Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);
                        var point = new Position(lat, lng, null);
                        geometry = new Point(new Position(point.Latitude, point.Longitude));
                        properies.Add("entityType", "DBText");
                        properies.Add("entityName", ((DBText)ent).TextString);
                        feature = new Feature((Point)geometry, properies, null);
                    }

                    //new TypedFeatureProps() { Name = "Style", Value = ent.Color.ToMediaColor().ToString() };
                    featureCollection.Features.Add(feature);

                });
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                var a = ex;
            }

            if (featureCollection.Features.Any())
            {
                Autodesk.AutoCAD.Windows.SaveFileDialog saveFileDialog = new Autodesk.AutoCAD.Windows.SaveFileDialog("save file", fileName, "json", "IntellidesK",
                    Autodesk.AutoCAD.Windows.SaveFileDialog.SaveFileDialogFlags.DoNotWarnIfFileExist);

                var dialogResult = saveFileDialog.ShowDialog();

                if (dialogResult == DialogResult.OK && saveFileDialog.Filename != "")
                {
                    //System.Text.StringBuilder sb = new System.Text.StringBuilder();

                    string jsonResult = JsonConvert.SerializeObject(featureCollection);

                    //sb.AppendLine(geometryJson);
                    File.WriteAllText(saveFileDialog.Filename, jsonResult);
                }
            }

            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }

        public static void XSaveDataAsGeoJson(this List<ObjectId> ids)
        {
            Entity ent;
            GeoJSONObject geometry;
            Feature feature = null;
            FeatureCollection featureCollection = new FeatureCollection();
            string fileName = "export_data";
            try
            {
                ids.ForEach(id =>
                {
                    ent = id.XGetEntity();
                    if (ent == null)
                    {
                        return;
                    }

                    var properies = new Dictionary<string, object> { { "color", ent.Color.ToHtmlColor() } };

                    if (ent.GetType().BaseType == typeof(Curve))
                    {
                        fileName = "lines_data.json";
                        var points = ent.XGetPoints();
                        List<IPosition> positions = new List<IPosition>();
                        foreach (Point3d pnt in points)
                        {
                            double lat, lng;
                            Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);
                            if (positions.Count() <= 2 || !positions.Any(x => x.Latitude == lat && x.Longitude == lng))
                                positions.Add(new Position(lat, lng, null));
                        }
                        geometry = new LineString(positions);
                        properies.Add("EntityType", ent.GetType().Name);

                        Type type = ent.XGetXDataObjectType();
                        if (type != null)
                        {
                            IElementDefinition element = ent.XGetXDataObject();
                            if (element != null)
                                properies.Add("entityName", element.Name);
                        }

                        //if (type == typeof(AcadTitle))
                        //    element = ent.XGetXDataObject();
                        //if (type == typeof(AcadCable))
                        //    element = ent.XGetXDataObject<AcadCable>();
                        //else if (type == typeof(AcadClosure))
                        //    element = ent.XGetXDataObject<AcadClosure>();
                        //else if (type == typeof(AcadCabinet))
                        //    element = ent.XGetXDataObject<AcadCabinet>();

                        feature = new Feature((LineString)geometry, properies, null);
                    }
                    else if (ent.GetType() == typeof(BlockReference))
                    {
                        fileName = "markers_data.json";
                        Point3d pnt = ent.XGetBasePoint();
                        double lat, lng;
                        Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);
                        var point = new Position(lat, lng, null);
                        geometry = new Point(new Position(point.Latitude, point.Longitude)); //mapinfo need for change: Longitude <--> Latitude
                        properies.Add("entityType", "BlockReference");

                        var br = (BlockReference)ent;
                        properies.Add("entityName", br.Name);

                        if (br.AttributeCollection != null)
                        {
                            using (var scope = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.TransactionScope())
                            {
                                var columns = new Dictionary<string, string>();
                                foreach (ObjectId attrId in br.AttributeCollection)
                                {
                                    AttributeReference attr = (AttributeReference)scope.tr.GetObject(attrId, OpenMode.ForRead);
                                    if (!columns.Keys.Contains(attr.Tag.ToLower()))
                                        columns.Add(attr.Tag.ToLower(), attr.TextString);
                                }
                                properies.Add("entityValue", JsonConvert.SerializeObject(columns));
                            }
                        }

                        feature = new Feature((Point)geometry, properies, null);
                    }
                    else if (ent.GetType() == typeof(DBText))
                    {
                        fileName = "text_data.json";
                        Point3d pnt = ent.XGetBasePoint();
                        double lat, lng;
                        Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);
                        var point = new Position(lat, lng, null);
                        geometry = new Point(new Position(point.Latitude, point.Longitude));
                        properies.Add("entityType", "DBText");
                        properies.Add("entityName", ((DBText)ent).TextString);
                        feature = new Feature((Point)geometry, properies, null);
                    }

                    //new TypedFeatureProps() { Name = "Style", Value = ent.Color.ToMediaColor().ToString() };
                    featureCollection.Features.Add(feature);

                });
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                var a = ex;
            }

            if (featureCollection.Features.Any())
            {
                Autodesk.AutoCAD.Windows.SaveFileDialog saveFileDialog = new Autodesk.AutoCAD.Windows.SaveFileDialog("save file", fileName, "json", "IntellidesK",
                    Autodesk.AutoCAD.Windows.SaveFileDialog.SaveFileDialogFlags.DoNotWarnIfFileExist);

                var dialogResult = saveFileDialog.ShowDialog();

                if (dialogResult == DialogResult.OK && saveFileDialog.Filename != "")
                {
                    //System.Text.StringBuilder sb = new System.Text.StringBuilder();

                    string jsonResult = JsonConvert.SerializeObject(featureCollection);

                    //sb.AppendLine(geometryJson);
                    File.WriteAllText(saveFileDialog.Filename, jsonResult);
                }
            }

            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }

        public static string ToHtmlColor(this Color acadColor)
        {
            System.Drawing.Color color = acadColor.ColorValue;
            //string hex = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return System.Drawing.ColorTranslator.ToHtml(color);
        }

        public static void ShapeIntersectsRect(string filePath, double x, double y, double width, double height)
        {
            //ShapeFile sf = new ShapeFile(filePath);
            //sf.RenderSettings.FieldName = sf.RenderSettings.DbfReader.GetFieldNames()[0];
            //sf.RenderSettings.UseToolTip = true;
            //sf.RenderSettings.ToolTipFieldName = sf.RenderSettings.FieldName;
            //sf.RenderSettings.IsSelectable = true;

            //PointD location = new PointD(x, y);
            //SizeD size = new SizeD(width, height);
            //RectangleD rec = new RectangleD(location, size);

            //sf.ShapeIntersectsRect(0, ref rec);
            //for (int i = 0; i < sf.RecordCount; i++)
            //{
            //    sf.SelectRecord(i, true);
            //}
            //sf.GetShapeIndiciesIntersectingRect(new List<int>() { 0 }, rec);
        }
    }
}
