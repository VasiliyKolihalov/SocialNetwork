using AutoMapper;
using SocialNetwork.Models.Correspondences;

namespace SocialNetwork.Profiles;

public class CorrespondenceProfile : Profile
{
    public CorrespondenceProfile()
    {
        CreateMap<Correspondence, CorrespondencePreviewModel>();
        CreateMap<Correspondence, CorrespondenceViewModel>();
        CreateMap<CorrespondenceAddModel, Correspondence>();
        CreateMap<CorrespondenceEditModel, Correspondence>();
    }
}