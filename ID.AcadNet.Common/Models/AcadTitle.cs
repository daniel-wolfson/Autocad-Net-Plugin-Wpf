using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.Data.Models.Entities;
using Intellidesk.Data.Repositories.Infrastructure;
using Newtonsoft.Json;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadTitle : Title, IAcadElementDefinition
    {
        public Point3d BasePoint { get; set; }

        public AcadTitle() : base(eTitleType.Default, eTitleType.Default.GetDisplayName(), "")
        {
        }

        public AcadTitle(eTitleType titleType) : base(titleType, titleType.GetDisplayName(), "")
        {
        }

        public AcadTitle(eTitleType titleType, string name) : base(titleType, name, "")
        {
        }

        public AcadTitle(eTitleType titleType, string name, string handle) : base(titleType, name, handle)
        {
        }

        public TypedValue[] GetTypedValues()
        {
            return new[]
            {
                new TypedValue((int) DxfCode.Start, this.GetType().Name),
                new TypedValue((int) DxfCode.Text, this.Title + this.ElementName),
                new TypedValue((int) DxfCode.Handle, this.Handle),
                new TypedValue((int) DxfCode.LayerName, this.LayerId.ToString()),
                new TypedValue((int) DxfCode.Color, this.ColorIndex),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, JsonConvert.SerializeObject(this))
            };
        }

        public override bool Equals(object obj)
        {
            var acadCable = obj as AcadCable;
            if (acadCable != null)
            {
                // Return true if the fields match:
                return acadCable.Title == Title;
            }
            return false;
        }

        public string IsVisible
        {
            get
            {
                if (Title.Contains("Select"))
                    return "Collapsed";
                return "Visible";
            }
        }

        Point3d IAcadElementDefinition.BasePoint { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public ObjectIdItem GetListItem(DBObject obj)
        {
            if (obj is DBText)
            {
                var title = (DBText)obj;
                return new ObjectIdItem(title.ObjectId, $"Title: {title.TextString}");
            }
            return null;
        }

        public void UpgradeDbObject(DBObject dbObject, IPaletteElement elementPrototype, ObjectState objectState = ObjectState.Unchanged)
        {
            var db = dbObject.Database;
            //Intellidesk.AcadNet.Services.Draw.CurrentLayerName = Intellidesk.AcadNet.Services.Draw.SetLayer(elementPrototype.LayerName);

            //using (db.TransactionManager.TransactionScope())
            //{
            //    IElementDefinition element;
            //    if (dbObject.GetType() == typeof(DBText))
            //    {
            //        DBText entity = (DBText)db.XGetObject(dbObject.Handle.ToString());
            //        entity.UpgradeOpen();
            //        entity.TextString = elementPrototype.Name;
            //        entity.Color = Color.FromColorIndex(ColorMethod.ByAci, elementPrototype.ColorIndex);
            //        entity.Layer = elementPrototype.LayerName;
            //        element = dbObject.XGetXDataObject<AcadTitle>();
            //        element.UpdateType(elementPrototype, objectState);
            //        dbObject.XAddXData(element);
            //        entity.DowngradeOpen();
            //    }
            //    else
            //    {
            //        return;
            //    }

            //    if (element.Items.Length > 0)
            //    {
            //        foreach (var itemHandle in element.Items)
            //        {
            //            DBObject entity = db.XGetObject(itemHandle);
            //            entity.XUpgradeObject(elementPrototype, objectState);
            //        }
            //    }
            //}
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        TypedValue[] IAcadElementDefinition.GetTypedValues()
        {
            throw new System.NotImplementedException();
        }

        //public TextHorizontalMode TextAlign { get; set; }
    }
}