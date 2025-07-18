using AutoMapper;
using ID.Infrastructure.Interfaces;
using ID.WebApi.Models.Identity;

namespace ID.Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<AppUserModel, Users>();
            CreateMap<Users, AppUserModel>();

            CreateMap<AspNetUserRoles, IUserRoles>();
            CreateMap<IUserRoles, AspNetUserRoles>();

            CreateMap<AspNetUserClaims, IUserClaims>();
            CreateMap<IUserClaims, AspNetUserClaims>();

            CreateMap<AspNetUsers, IUserDetails>();
            CreateMap<IUserDetails, AspNetUsers>();

            CreateMap<Users, IUserDetails>();
            CreateMap<IUserDetails, Users>();
        }
    }
}
