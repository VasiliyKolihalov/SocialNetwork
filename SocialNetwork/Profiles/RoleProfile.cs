using AutoMapper;
using SocialNetwork.Models.Roles;

namespace SocialNetwork.Profiles;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role, RoleViewModel>();
        CreateMap<RoleAddModel, Role>();
        CreateMap<RoleEditModel, Role>();
    }
}