using AutoMapper;
using SocialNetwork.Models.Comments;

namespace SocialNetwork.Profiles;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<Comment, CommentViewModel>();
        CreateMap<CommentAddModel, Comment>();
        CreateMap<CommentEditModel, Comment>();
    }
}