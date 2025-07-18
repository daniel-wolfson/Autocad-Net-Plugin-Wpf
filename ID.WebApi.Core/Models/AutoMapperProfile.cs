using AutoMapper;

namespace ID.WebApi.Models.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CreateMap<AspNetUsers, UserDetails>()
            //    .ForMember(dest => dest.form_guid, opt => opt.MapFrom(src => src.FormGuid))
            //    .ForMember(dest => dest.form_template_guid, opt => opt.MapFrom(src => src.FormTemplateGuid))
            //    .ForMember(dest => dest.activity_guid, opt => opt.MapFrom(src => src.ActivityGuid))
            //    .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.FormTemplateGu.Name))
            //    .ForMember(dest => dest.approve_user_guid, opt => opt.MapFrom(src => src.ApproveUserGuid))
            //    .ForMember(dest => dest.approve_date, opt => opt.MapFrom(src => src.ApproveDate))
            //    .ForMember(dest => dest.status, opt => opt.MapFrom(src => src.Status))
            //    .ForMember(dest => dest.org_obj_guid, opt => opt.MapFrom(src => src.OrgObjGuid))
            //    .ForMember(dest => dest.org_obj_name, opt => opt.MapFrom(src => src.OrgObjGu.Name))
            //    .ReverseMap();
        }
    }
}