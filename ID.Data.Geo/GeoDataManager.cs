using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.Data.Gis;
using System.Collections.Generic;
using System.Linq;
using Intellidesk.AcadNet.Services.Extentions;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ID.Data.Geo
{
    public class GeoDataManager
    {
        public static void ExecuteSaveAsCommand(List<ObjectId> ids)
        {
            FeatureCollection featureCollection = new FeatureCollection();
            Point3dCollection entityPoits;
            List<IPosition> positions;
            LineString geometry;

            Entity ent;
            ids.ForEach(id =>
            {
                ent = id.XGetEntity();
                entityPoits = ent.XGetPointsVertices();
                //geometries = new List<IGeometryObject>();
                positions = new List<IPosition>();

                foreach (Point3d entityPoit in entityPoits)
                {
                    double lat, lng;
                    Converters.itm2wgs84((int)entityPoit.Y, (int)entityPoit.X, out lat, out lng);
                    positions.Add(new Position(lat, lng, null));
                }

                geometry = new LineString(positions);
                var properies = new Dictionary<string, object> { { "color", ent.Color.ToHtmlColor() } };
                //new TypedFeatureProps() { Name = "Style", Value = ent.Color.ToMediaColor().ToString() };
                Feature feature = new Feature(geometry, properies, null);
                featureCollection.Features.Add(feature);

            });

            if (featureCollection.Features.Any())
            {
                Autodesk.AutoCAD.Windows.SaveFileDialog saveFileDialog = new Autodesk.AutoCAD.Windows.SaveFileDialog("save file", "kav_hafira", "json", "IntellidesK",
                    Autodesk.AutoCAD.Windows.SaveFileDialog.SaveFileDialogFlags.DoNotWarnIfFileExist);

                DialogResult dialogResult = saveFileDialog.ShowDialog();

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
