using ID.Infrastructure.Enums;
using Intellidesk.Data.Repositories.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;

namespace Intellidesk.Data.Models.Entities
{
    public interface IPaletteElement
    {
        string Title { get; set; }
        string ElementName { get; set; }
        string ElementType { get; set; }
        int BodyType { get; set; }
        int? TypeCode { get; set; }
        string TypeCodeFullName { get; set; }
        int? TitleColorIndex { get; set; }
        int? ColorIndex { get; set; }
        string LayerName { get; set; }
        string Handle { get; set; }
        double? Height { get; set; }
        double? Width { get; set; }
        short? Weight { get; set; }

        string OwnerHandle { get; set; }
        string OwnerFullType { get; set; }
        string ParentHandle { get; set; }
        short PaletteType { get; set; }
        string[] Items { get; set; }
        bool IsOwner { get; }
        double? Rotation { get; set; }
        int? LayoutId { get; set; }
        TextHorizontalMode TextAlign { get; set; }
        ObjectState ObjectState { get; set; }
        Dictionary<string, DbGeometry> Geometry { get; set; }

        T Update<T>(T updateParams, ObjectState objectState = ObjectState.Unchanged) where T : IPaletteElement;
        IPaletteElement Update(Enum eType, ObjectState objectState = ObjectState.Unchanged);
        T Extend<T>(Func<T, T> fn) where T : IPaletteElement;
        DataInfoAttribute GetDataInfo();
    }
}