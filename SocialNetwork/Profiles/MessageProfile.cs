using AutoMapper;
using SocialNetwork.Models.Messages;

namespace SocialNetwork.Profiles;

public class MessageProfile : Profile
{
    public MessageProfile()
    {
        CreateMap<Message, MessageViewModel>();
        CreateMap<MessageAddModel, Message>();
        CreateMap<MessageEditModel, Message>();
    }
}