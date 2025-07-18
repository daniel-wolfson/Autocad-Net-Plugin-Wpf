using ID.Infrastructure.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ID.Infrastructure.Models
{
    public class ConcreteTypeConverter<TConcrete> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //assume we can convert to anything for now
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //explicitly specify the concrete type we want to create
            return serializer.Deserialize<TConcrete>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //use the default serialization - it works fine
            serializer.Serialize(writer, value);
        }
    }

    public class UserOrganizationsRolesExt : IUserOrganizationsRole
    {
        public int UserId { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        [JsonConverter(typeof(ConcreteTypeConverter<IEnumerable<RolePermissionsTabsInfoExt>>))]
        public IEnumerable<IAppPermissionsInfo> Permissions { get; set; }
    }

    public class RolePermissionsTabsInfoExt : IAppPermissionsInfo
    {
        public int TabId { get; set; }
        public string TabName { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public bool? IsActive { get; set; }
        public int TabOrder { get; set; }

        public int? RoleId { get; set; }
        public int PermissionTypeId { get; set; }
        public string PermissionTypeName { get; set; }
    }

    public class AbstractConverter<TReal, TAbstract> : JsonConverter where TReal : TAbstract
    {
        public override Boolean CanConvert(Type objectType)
            => objectType == typeof(TAbstract);

        public override Object ReadJson(JsonReader reader, Type type, Object value, JsonSerializer jser)
            => jser.Deserialize<TReal>(reader);

        public override void WriteJson(JsonWriter writer, Object value, JsonSerializer jser)
            => jser.Serialize(writer, value);
    }
}
