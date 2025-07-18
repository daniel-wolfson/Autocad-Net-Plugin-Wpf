using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using Intellidesk.Data.Repositories.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Spatial;
using System.Linq;
using AttributeDefinition = Intellidesk.Data.Models.Cad.AttributeDefinition;
using ILayout = Intellidesk.Data.Models.Cad.ILayout;
using TextHorizontalMode = ID.Infrastructure.Enums.TextHorizontalMode;

namespace Intellidesk.Data.Models.Entities
{
    [Serializable]
    public class PaletteElement : BaseEntity, IPaletteElement
    {
        #region <pops>

        private string _title;

        public string Handle { get; set; }
        public object ObjectId { get; set; }
        public string[] Items { get; set; } = { };
        public string OwnerHandle { get; set; }
        public string OwnerFullType { get; set; }
        public string ParentHandle { get; set; }
        public int BodyType { get; set; }
        public int? TypeCode { get; set; }
        public string TypeCodeFullName { get; set; }
        public string ElementName { get; set; }
        public string ElementType { get; set; }
        public short PaletteType { get; set; }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        public string FileName { get; set; }
        public double? Width { get; set; } = 6;
        public double? Height { get; set; } = 6;
        public short? Weight { get; set; }

        public int? ColorIndex { get; set; }
        public int? TitleColorIndex { get; set; }
        public double? Rotation { get; set; }
        public long? LayerId { get; set; }
        public string LayerName { get; set; } = "0";
        public int? LayoutId { get; set; }
        public int? TabIndex { get; set; }
        public TextHorizontalMode TextAlign { get; set; } = TextHorizontalMode.TextLeft;
        public string ElementGroupId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public List<Title> Titles { get; set; }
        public ICollection<AttributeDefinition> Attributes { get; set; }
        public DbGeography Location { get; set; }
        public string XrefName { get; set; }
        [DefaultValue(1.0)] public virtual double Scale { get; set; }

        public ILayout Layout { get; set; }
        public bool IsOwner => Handle == OwnerHandle || string.IsNullOrEmpty(OwnerHandle);
        public object Target { get; set; }

        public new virtual ObjectState ObjectState { get; set; } = ObjectState.Added;
        public Dictionary<string, DbGeometry> Geometry { get; set; }
        public Dictionary<string, DbGeometry> Geometries { get; set; }

        #endregion <pops>

        #region <ctor>
        public PaletteElement()
        {
            Titles = new List<Title>();
            this.Attributes = new List<AttributeDefinition>();
            //var geom = DbGeometry.PolygonFromText("POLYGON((127.652 -26.244,127.652 -26.194,127.652 -26.244))", 4326);
            //Geometry = new Dictionary<string, DbGeometry>() {{"main", geom } };
            DateCreated = DateModified = DateTime.Now;
        }
        public PaletteElement(Type type, int? typeCode) : base()
        {
            var dataInfo = type.GetAttributes<DataInfoAttribute>()
                .FirstOrDefault(x => x.Key == typeCode).Value;

            ElementName = dataInfo.Name;
            ElementType = this.GetType().FullName;
            ColorIndex = dataInfo.ColorIndex;
            TitleColorIndex = dataInfo.TitleColorIndex;
            LayerName = dataInfo.LayerName;
            TypeCode = typeCode;
            TypeCodeFullName = type.FullName;

            TextAlign = TextHorizontalMode.TextMid;
        }
        public PaletteElement(Type ownerType, Type type, int? typeCode) : base()
        {
            OwnerFullType = ownerType.FullName;
        }

        public PaletteElement(Enum typeCode) : base()
        {
            var type = typeCode.GetType();
            //var vv = Enum.ToObject(type, typeCode);
            var dataInfo = type.GetAttributes<DataInfoAttribute>()
                .FirstOrDefault(x => x.Key == (int)(object)typeCode).Value;

            ElementName = dataInfo.Name;
            ElementType = this.GetType().FullName;
            TypeCode = (int)(object)typeCode;
            TypeCodeFullName = type.FullName;
            ColorIndex = dataInfo.ColorIndex;
            TitleColorIndex = dataInfo.TitleColorIndex;
            LayerName = dataInfo.LayerName;
            TextAlign = TextHorizontalMode.TextMid;
        }

        #endregion <ctor>

        #region <abstract methods>

        //[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        //public abstract void GetObjectData(SerializationInfo info, StreamingContext context);

        #endregion <abstract methods>

        #region <virtual methods>

        public virtual void InitFromType<T>(T prototype) where T : PaletteElement
        {
            this.ColorIndex = prototype.ColorIndex;
            this.Geometry = prototype.Geometry;
            this.LayerId = prototype.LayerId;
            this.TypeCode = prototype.TypeCode;
        }

        public virtual T SetUpdateType<T>(Enum eType) where T : PaletteElement
        {
            return (T)this; //TODO
        }

        public virtual object GetListItem()
        {
            return null;
        }

        public new T Clone<T>() where T : IPaletteElement
        {
            T other = (T)this.MemberwiseClone();
            return other;
        }

        public new virtual PaletteElement Clone()
        {
            PaletteElement other = this.MemberwiseClone() as PaletteElement;
            return other;
        }

        public PaletteElement Extend(Func<PaletteElement, PaletteElement> fn)
        {
            return fn(this.Clone());
        }

        public T Extend<T>(Func<T, T> fn) where T : IPaletteElement
        {
            return fn(this.Clone<T>());
        }

        public T Update<T>(T updateParams, ObjectState objectState = ObjectState.Unchanged)
            where T : IPaletteElement
        {
            //var elementType = element.GetType().Name;
            //var typeCodeValue = element.GetType().CustomAttributes.First().ConstructorArguments.First().Value;
            //var typeNameValue = element.GetType().CustomAttributes.First().ConstructorArguments.Last().Value;

            if (!string.IsNullOrEmpty(updateParams.OwnerHandle))
                OwnerHandle = updateParams.OwnerHandle;

            if (!string.IsNullOrEmpty(updateParams.OwnerFullType))
                OwnerFullType = updateParams.OwnerFullType;

            if (!string.IsNullOrEmpty(updateParams.ElementType))
                ElementType = updateParams.ElementType;

            if (!string.IsNullOrEmpty(updateParams.ElementName))
                ElementName = updateParams.ElementName;

            if (updateParams.TypeCode.HasValue)
                TypeCode = updateParams.TypeCode;

            if (updateParams.ColorIndex.HasValue)
                ColorIndex = updateParams.ColorIndex; // eType.GetDataInfo().ColorIndex;

            if (updateParams.TitleColorIndex.HasValue)
                TitleColorIndex = updateParams.TitleColorIndex; // eType.GetDataInfo().TitleColorIndex;

            Title = updateParams.Title;
            ElementName = updateParams.GetType().Name; //eType.GetDisplayName();
            ElementType = updateParams.GetType().FullName;
            LayerName = updateParams.LayerName; //eType.GetDataInfo().LayerName;
            ObjectState = objectState;
            return updateParams;
        }

        public virtual IPaletteElement Update(Type type, int? eTypeCode, ObjectState objectState = ObjectState.Unchanged)
        {
            DataInfoAttribute dataInfo = type.GetAttributes<DataInfoAttribute>()
                .FirstOrDefault(x => x.Key == (eTypeCode ?? 0)).Value;

            if (this.ObjectState != ObjectState.Edit)
                Title = dataInfo.Name;

            TypeCode = (int?)Enum.ToObject(type, eTypeCode ?? 0);
            ElementName = dataInfo.Name;
            ColorIndex = dataInfo.ColorIndex;
            TitleColorIndex = dataInfo.TitleColorIndex;
            LayerName = dataInfo.LayerName;
            ObjectState = objectState;
            return this;
        }

        public virtual IPaletteElement Update(Enum eType, ObjectState objectState = ObjectState.Unchanged)
        {
            TypeCode = (short)Convert.ChangeType(eType, typeof(short));
            ElementName = eType.GetDisplayName();
            ColorIndex = eType.GetDataInfo().ColorIndex;
            TitleColorIndex = eType.GetDataInfo().TitleColorIndex;
            LayerName = eType.GetDataInfo().LayerName;
            ObjectState = objectState;
            return this;
        }

        public virtual T Update<T>(Func<T, T> fn) where T : PaletteElement
        {
            ObjectState = ObjectState != ObjectState.Added ? ObjectState.Modified : ObjectState;
            return fn((T)this);
        }

        public virtual DataInfoAttribute GetDataInfo()
        {
            var type = Type.GetType(this.ElementType);
            var dataInfo = type.GetAttributes<DataInfoAttribute>()
                .FirstOrDefault(x => x.Key == this.TypeCode).Value;
            return dataInfo;
        }

        #endregion <public methods>
    }
}