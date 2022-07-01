using AutoMapper;
using SocialNetwork.Models.FriendsRequests;

namespace SocialNetwork.Profiles;

public class FriendsRequestProfile : Profile
{
    public FriendsRequestProfile()
    {
        CreateMap<FriendRequest, FriendsRequestViewModel>();
        CreateMap<FriendRequestAddModel, FriendRequest>();
    }
}