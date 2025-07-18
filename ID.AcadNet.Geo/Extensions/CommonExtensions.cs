using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using EGIS.ShapeFileLib;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Geo.Models;
using Intellidesk.Data.Gis;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using EsriFeatureCollection = Intellidesk.AcadNet.Geo.Models.EsriFeatureCollection;

namespace Intellidesk.AcadNet.Geo.Extensions
{
    public enum eGeoCommands
    {
        LineString, MultiLineString, Point, MultiPoint, Poligon, MultiPoligon
    }

    public static class CommonExtensions
    {
        public static void XSaveAsGeoJson(this List<ObjectId> ids, ICommandArgs commandArgs)
        {
            EsriFeatureCollection featureCollection = new EsriFeatureCollection
            {
                Metadata = new MetaData()
                {
                    generated = 1566072520000,
                    url = "http://localhost:4200/assets/markers_data.geojson",
                    title = "USGS All Earthquakes, Past Month",
                    status = 200,
                    api = "1.8.1",
                    count = 19696
                },
                Bbox = new double[] { -179.9736, -61.2936, -3.59, 179.9212, 72.1251, 664.54 }
            };

            string fileName = "export_data";
            try
            {
                IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
                foreach (var id in ids)
                {
                    Entity ent = id.XGetEntity();
                    if (ent == null || commandArgs.CancelToken.IsCancellationRequested)
                        continue;

                    Feature feature = ent.ToFeature();
                    featureCollection.Features.Add(feature);
                };
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Log.Logger.Error(ex?.InnerException?.Message ?? ex.Message);
            }

            if (featureCollection.Features.Any())
            {
                Autodesk.AutoCAD.Windows.SaveFileDialog saveFileDialog =
                    new Autodesk.AutoCAD.Windows.SaveFileDialog("save file", fileName, "json", "IntellidesK",
                        Autodesk.AutoCAD.Windows.SaveFileDialog.SaveFileDialogFlags.DoNotWarnIfFileExist);

                var dialogResult = saveFileDialog.ShowDialog();
                if (dialogResult == DialogResult.OK && saveFileDialog.Filename != "")
                {
                    //System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    //sb.AppendLine(geometryJson);

                    string jsonResult = JsonConvert.SerializeObject(featureCollection);
                    File.WriteAllText(saveFileDialog.Filename, jsonResult.Replace(@"\", ""));
                }
            }

            Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();
        }

        public static void XSaveAsGeoJsonParallel(this List<ObjectId> ids, ICommandArgs commandArgs) //, CancellationTokenSource cts
        {
            EsriFeatureCollection featureCollection = new EsriFeatureCollection();
            featureCollection.Metadata = new MetaData()
            {
                generated = 1566072520000,
                url = "http://localhost:4200/assets/markers_data.geojson",
                title = "USGS All Earthquakes, Past Month",
                status = 200,
                api = "1.8.1",
                count = 19696
            };
            featureCollection.Bbox = new double[] { -179.9736, -61.2936, -3.59, 179.9212, 72.1251, 664.54 };

            //string fileName = "export_esri_data";

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = commandArgs.CancelToken;
            po.MaxDegreeOfParallelism = Environment.ProcessorCount;

            try
            {
                Parallel.ForEach(ids, () => new List<Feature>(), (id, loopState, featureList) =>
                {
                    po.CancellationToken.ThrowIfCancellationRequested();

                    Entity ent = id.XGetEntity();
                    if (ent == null)
                        return featureList;

                    //Console.WriteLine("{0} on {1}", d, Thread.CurrentThread.ManagedThreadId);
                    featureList.Add(ent.ToFeature());
                    return featureList;
                },
                featureList =>
                {
                    lock (featureCollection)
                        featureCollection.Features.AddRange(featureList);
                });
            }
            catch (OperationCanceledException)
            {
                //Console.WriteLine(e.Message);
            }
            finally
            {
                //cts.Dispose();
            }
        }

        public static Feature ToFeature(this Entity ent)
        {
            GeoJSONObject geometry;
            FeatureEsri feature = null;
            var properies = new Dictionary<string, object> { { "color", ent.Color.ToHtmlColor() } };

            if (ent.GetType().BaseType == typeof(Curve))
            {
                var points = ent.XGetPoints();
                List<IPosition> positions = new List<IPosition>();
                foreach (Point3d pnt in points)
                {
                    double lat, lng;
                    Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);

                    if (positions.Count() <= 2 || !positions.Any(x => x.Latitude == lat && x.Longitude == lng))
                        positions.Add(new Position(lat, lng, null));
                }
                geometry = new LineStringEsri(positions);
                properies.Add("mag", "4.0");
                properies.Add("EntityType", ent.GetType().Name);

                Type type = ent.XGetXDataObjectType();
                if (type != null)
                {
                    IPaletteElement element = ent.XGetDataObject();
                    if (element != null)
                        properies.Add("entityName", element.Title);
                }

                feature = new FeatureEsri((LineString)geometry, properies, ent.Handle.ToString());
            }
            else if (ent.GetType() == typeof(BlockReference))
            {
                Point3d pnt = ent.XGetBasePoint();
                double lat, lng;
                Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);
                var pos = new Position(lat, lng, null);
                geometry = new PointEsri(new Position(pos.Latitude, pos.Longitude)); //mapinfo need for change: Longitude <--> Latitude
                properies.Add("mag", "4.0");
                properies.Add("entityType", ent.GetType().Name);

                var br = (BlockReference)ent;
                properies.Add("entityName", br.Name);

                if (br.AttributeCollection != null)
                {
                    using (var tr = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
                    {
                        var columns = new Dictionary<string, string>();
                        foreach (ObjectId attrId in br.AttributeCollection)
                        {
                            AttributeReference attr = (AttributeReference)tr.GetObject(attrId, OpenMode.ForRead);
                            if (!properies.Keys.Contains(attr.Tag.ToLower()))
                                properies.Add(attr.Tag.ToLower(), attr.TextString);
                        }
                    }
                }

                feature = new FeatureEsri((Point)geometry, properies, ent.Handle.ToString());
            }
            else if (ent.GetType() == typeof(DBText))
            {
                Point3d pnt = ent.XGetBasePoint();
                double lat, lng;
                Converters.itm2wgs84((int)pnt.Y, (int)pnt.X, out lat, out lng);
                var pos = new Position(lat, lng, null);
                geometry = new PointEsri(new Position(pos.Latitude, pos.Longitude));
                properies.Add("entityType", "DBText");
                properies.Add("entityName", ((DBText)ent).TextString);
                feature = new FeatureEsri((Point)geometry, properies, ent.Handle.ToString());
            }

            return feature;
        }

        public static string ToHtmlColor(this Color acadColor)
        {
            System.Drawing.Color color = acadColor.ColorValue;
            //string hex = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return System.Drawing.ColorTranslator.ToHtml(color);
        }

        public static void ShapeIntersectsRect(string filePath, double x, double y, double width, double height)
        {
            ShapeFile sf = new ShapeFile(filePath);
            sf.RenderSettings.FieldName = sf.RenderSettings.DbfReader.GetFieldNames()[0];
            sf.RenderSettings.UseToolTip = true;
            sf.RenderSettings.ToolTipFieldName = sf.RenderSettings.FieldName;
            sf.RenderSettings.IsSelectable = true;

            PointD location = new PointD(x, y);
            SizeD size = new SizeD(width, height);
            RectangleD rec = new RectangleD(location, size);

            sf.ShapeIntersectsRect(0, ref rec);
            for (int i = 0; i < sf.RecordCount; i++)
            {
                sf.SelectRecord(i, true);
            }
            sf.GetShapeIndiciesIntersectingRect(new List<int>() { 0 }, rec);
        }
    }
}
