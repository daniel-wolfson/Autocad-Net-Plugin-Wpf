using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Color = Autodesk.AutoCAD.Colors.Color;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadLayers : ObservableCollection<AcadLayer>
    {
        public AcadLayers(IPaletteElement[] elementTypes = null)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                foreach (ObjectId ltrId in lt)
                {
                    LayerTableRecord ltr = tr.GetObject(ltrId, OpenMode.ForRead) as LayerTableRecord;
                    Color color = ltr.Color;
                    AcadLayer acadLayer;
                    if (!color.IsByAci)
                    {
                        if (color.IsByLayer)
                            acadLayer = new AcadLayer(ltr.Handle.ToString(), ltr.Name, 255, !ltr.IsOff, ltr.IsFrozen, ltr.IsLocked);
                        else if (color.IsByBlock)
                            acadLayer = new AcadLayer(ltr.Handle.ToString(), ltr.Name, 255, !ltr.IsOff, ltr.IsFrozen, ltr.IsLocked);
                        else
                            acadLayer = new AcadLayer(ltr.Handle.ToString(), ltr.Name, color.ColorIndex, !ltr.IsOff, ltr.IsFrozen, ltr.IsLocked);
                    }
                    else
                    {
                        short colIndex = color.ColorIndex;

                        byte byt = Convert.ToByte(colIndex);
                        int rgb = EntityColor.LookUpRgb(byt);
                        long b = rgb & 0xffL;
                        long g = (rgb & 0xff00L) >> 8;
                        long r = rgb >> 16;

                        if (colIndex == 7)
                        {
                            if (r <= 128 && g <= 128 && b <= 128)
                                acadLayer = new AcadLayer(ltr.Handle.ToString(), ltr.Name, color.ColorIndex, !ltr.IsOff, ltr.IsFrozen, ltr.IsLocked); // White
                            else
                                acadLayer = new AcadLayer(ltr.Handle.ToString(), ltr.Name, 255, !ltr.IsOff, ltr.IsFrozen, ltr.IsLocked); // Black
                        }
                        else
                            acadLayer = new AcadLayer(ltr.Handle.ToString(), ltr.Name, 255, !ltr.IsOff, ltr.IsFrozen, ltr.IsLocked);
                    }

                    acadLayer.ObjectState = ObjectState.Unchanged;
                    Add(acadLayer);
                }
                tr.Commit();
            }

            if (elementTypes != null)
                AddLayers(elementTypes);
        }

        public void AddLayers<T>(T[] elementTypes) where T : IPaletteElement
        {
            foreach (var type in elementTypes)
            {
                Add(new AcadLayer(type));
            }
        }

        public void AddLayers(IPaletteElement[] elements)
        {
            var items = elements.Select(element => new AcadLayer(element));
            this.AddRange(items);
        }

        public List<AcadLayer> GetLayers<T>() where T : IPaletteElement
        {
            return Items.Where(x => x.LayerType != null && x.LayerType == typeof(T).Name).ToList();
        }

        public AcadLayer Get(string layerName)
        {
            return this.Any(x => x.LayerName.ToLower() == layerName.ToLower())
                ? this.FirstOrDefault(x => x.LayerName.ToLower() == layerName.ToLower())
                : this.FirstOrDefault(x => x.LayerName == "0");
        }
    }
}