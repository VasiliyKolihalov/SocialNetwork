using AutoMapper;
using SocialNetwork.Models.Posts;

namespace SocialNetwork.Profiles;

public class PostProfile : Profile
{
    public PostProfile()
    {
        CreateMap<Post, PostViewModel>();
        CreateMap<PostAddModel, Post>();
        CreateMap<PostEditModel, Post>();
    }
}