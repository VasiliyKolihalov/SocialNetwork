using AutoMapper;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserPreviewModel>();
        CreateMap<User, UserViewModel>();
        CreateMap<RegisterUserModel, User>();
        CreateMap<UserEditModel, User>();
       
    }
}