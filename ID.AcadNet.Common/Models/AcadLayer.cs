using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using System.Windows.Media;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Color = Autodesk.AutoCAD.Colors.Color;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadLayer : IObjectState
    {
        private bool _isOn;
        private bool _isFrozen;
        private bool _isLocked;

        public string Handle { get; set; }

        public string LayerName
        {
            get;
            set;
        }

        public short ColorIndex { get; set; }

        public ObjectId LayerId { get; set; }

        public SolidColorBrush LayerColorBrush
        {
            get
            {
                Color color = Color.FromColorIndex(ColorMethod.ByAci, ColorIndex);
                SolidColorBrush brush = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(color.ColorValue.R, color.ColorValue.G, color.ColorValue.B));
                return brush;
            }
        }

        public string IsOn
        {
            get
            {
                if (string.IsNullOrEmpty(this.Handle)) return "NotHandled";

                Document doc = acadApp.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    LayerTableRecord ltr = tr.GetObject(lt[LayerName], OpenMode.ForRead) as LayerTableRecord;
                    _isOn = !ltr.IsOff;
                    tr.Commit();
                }

                if (_isOn)
                    return "Visible";

                return "Collapsed";
            }
        }

        public string IsOff
        {
            get
            {
                if (string.IsNullOrEmpty(this.Handle)) return "NotHandled";

                Document doc = acadApp.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    LayerTableRecord ltr = tr.GetObject(lt[LayerName], OpenMode.ForRead) as LayerTableRecord;
                    _isOn = !ltr.IsOff;
                    tr.Commit();
                }

                if (!_isOn)
                    return "Visible";

                return "Collapsed";
            }
        }

        public string IsFrozen
        {
            get
            {
                if (string.IsNullOrEmpty(this.Handle)) return "NotHandled";

                Document doc = acadApp.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    LayerTableRecord ltr = tr.GetObject(lt[LayerName], OpenMode.ForRead) as LayerTableRecord;
                    _isFrozen = ltr.IsFrozen;
                    tr.Commit();
                }

                if (_isFrozen)
                    return "Visible";

                return "Collapsed";
            }
        }

        public string IsThawed
        {
            get
            {
                if (string.IsNullOrEmpty(this.Handle)) return "NotHandled";

                Document doc = acadApp.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    LayerTableRecord ltr = tr.GetObject(lt[LayerName], OpenMode.ForRead) as LayerTableRecord;
                    _isFrozen = ltr.IsFrozen;
                    tr.Commit();
                }

                if (!_isFrozen)
                    return "Visible";

                return "Collapsed";
            }
        }

        public string IsLocked
        {
            get
            {
                if (string.IsNullOrEmpty(this.Handle)) return "NotHandled";

                Document doc = acadApp.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    LayerTableRecord ltr = tr.GetObject(lt[LayerName], OpenMode.ForRead) as LayerTableRecord;
                    _isLocked = ltr.IsLocked;
                    tr.Commit();
                }

                if (_isLocked)
                    return "Visible";

                return "Collapsed";
            }
        }

        public string IsUnLocked
        {
            get
            {
                if (string.IsNullOrEmpty(this.Handle)) return "NotHandled";

                Document doc = acadApp.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    LayerTableRecord ltr = tr.GetObject(lt[LayerName], OpenMode.ForRead) as LayerTableRecord;
                    _isLocked = ltr.IsLocked;
                    tr.Commit();
                }

                if (!_isLocked)
                    return "Visible";

                return "Collapsed";
            }
        }

        public ObjectState ObjectState { get; set; } = ObjectState.Added;

        public string LayerType { get; set; }

        public AcadLayer(IPaletteElement element)
        {
            Handle = "";
            LayerType = element.ElementType;
            LayerName = element.LayerName;
            if (element.ColorIndex.HasValue)
            {
                var acadColor = Color.FromColorIndex(ColorMethod.ByAci, (short)element.ColorIndex);
                ColorIndex = acadColor.ColorIndex;
            }
            _isOn = true;
            _isFrozen = false;
            _isLocked = false;
        }

        public AcadLayer(string handle, string layerName, short colorIndex = 7, bool isOn = true, bool isFrozen = false, bool isLocked = false)
        {
            Handle = handle;
            LayerName = layerName;
            LayerType = null;
            ColorIndex = colorIndex;
            _isOn = isOn;
            _isFrozen = isFrozen;
            _isLocked = isLocked;
        }

        public override bool Equals(object obj)
        {
            AcadLayer layer = obj as AcadLayer;
            if (layer != null)
            {
                // Return true if the fields match:
                return (layer.LayerName == LayerName);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}