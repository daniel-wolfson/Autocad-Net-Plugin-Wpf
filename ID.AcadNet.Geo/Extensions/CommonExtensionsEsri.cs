using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using GeoJSON.Net.Geometry;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.Data.Gis;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;

using GeoJSON.Net;
using GeoJSON.Net.Feature;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using FeatureCollection = Intellidesk.AcadNet.Commands.Models.FeatureCollection;

namespace Intellidesk.AcadNet.Commands.Extensions
{
    public static class CommonExtensionsEsri
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
    }
}
