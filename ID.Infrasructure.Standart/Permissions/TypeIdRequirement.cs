using Microsoft.AspNetCore.Authorization;

namespace ID.Infrastructure.Permissions
{
    public class TypeIdRequirement : IAuthorizationRequirement
    {
        public TypeIdRequirement(int typeId)
        {
            TypeId = typeId;
        }

        public int TypeId { get; private set; }
    }
}
