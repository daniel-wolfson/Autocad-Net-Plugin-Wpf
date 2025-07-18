using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ID.Infrastructure.Extensions;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;
using System;
using TextHorizontalMode = ID.Infrastructure.Enums.TextHorizontalMode;

namespace Intellidesk.AcadNet.Common.Models
{
    public class AcadElement<TEntity, TEntityType> : PaletteElement, IPaletteElement
        where TEntity : PaletteElement
        where TEntityType : Enum
    {
        #region <props>
        public Point3d BasePoint { get; set; }
        public new double Height { get; set; } = 6;
        public new short Weight { get; set; }
        //public new int TypeCode { get; set; }
        public new int ColorIndex { get; set; }
        public new int TitleColorIndex { get; set; }
        public new double Rotation { get; set; }
        public new long LayerId { get; set; }
        public new int LayoutId { get; set; }
        public new int TabIndex { get; set; }
        #endregion <props>

        #region <ctor>

        public int ElementId { get; set; }

        public AcadElement()
        {
            TEntityType entityType = (TEntityType)(object)(base.TypeCode ?? 0);

            ElementName = default(TEntityType).GetDisplayName();
            base.TypeCode = default;
            ColorIndex = base.ColorIndex.HasValue ? (short)base.ColorIndex : entityType.GetDataInfo().ColorIndex;
            TitleColorIndex = entityType.GetDataInfo().TitleColorIndex;
            LayerName = entityType.GetDataInfo().LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        public AcadElement(TEntityType elementType, string title, string handle)
        {
            Title = title;
            Handle = handle;
            ElementName = elementType.GetDisplayName();
            base.TypeCode = (int)Convert.ChangeType(elementType, typeof(int)); ;
            ColorIndex = elementType.GetDataInfo().ColorIndex;
            TitleColorIndex = elementType.GetDataInfo().TitleColorIndex;
            LayerName = elementType.GetDataInfo().LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        public new TEntityType TypeCode
        {
            get => (TEntityType)(object)base.TypeCode;
            set => base.TypeCode = Convert.ToInt16(value);
        }

        #endregion <ctor>

        #region <methods>

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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}