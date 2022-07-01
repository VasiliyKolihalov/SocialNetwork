using AutoMapper;
using SocialNetwork.Models.Communities;

namespace SocialNetwork.Profiles;

public class CommunityProfile : Profile
{
    public CommunityProfile()
    {
        CreateMap<Community, CommunityPreviewModel>();
        CreateMap<Community, CommunityViewModel>();
        CreateMap<CommunityAddModel, Community>();
        CreateMap<CommunityEditModel, Community>();
    }
}