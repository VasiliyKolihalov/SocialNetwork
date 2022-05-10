using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Correspondences;
using SocialNetwork.Models.Messages;
using SocialNetwork.Models.Users;
using SocialNetwork.Repository;

namespace SocialNetwork.Services;

public class CorrespondencesService
{
    private readonly ApplicationContext _applicationContext;

    public CorrespondencesService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public IEnumerable<CorrespondencePreviewModel> GetAll(int userId)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<Message, MessageViewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        IEnumerable<Correspondence> correspondences =
            _applicationContext.Correspondences.GetUserCorrespondences(userId);

        IEnumerable<CorrespondencePreviewModel> correspondenceViewModels =
            mapper.Map<IEnumerable<Correspondence>, IEnumerable<CorrespondencePreviewModel>>(correspondences);
        return correspondenceViewModels;
    }

    public CorrespondenceViewModel GetWithMessages(int correspondenceId, int userId)
    {
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);
        
        if (!correspondence.Users.Any(x => x.Id == userId))
            throw new Exception("Correspondence not found");
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Correspondence, CorrespondenceViewModel>();
            cfg.CreateMap<Message, MessageViewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        CorrespondenceViewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondenceViewModel>(correspondence);

        correspondencePreviewModel.Messages =
            mapper.Map<IEnumerable<Message>, List<MessageViewModel>>(
                _applicationContext.Messages.GetFromCorrespondence(correspondenceId));

        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel StartCorrespondence(MessageAddModel messageAddModel, List<int> participantsId, int senderId)
    {
        User sender = _applicationContext.Users.Get(senderId);

        var participants = new List<User>();
        foreach (int id in participantsId)
        {
            if (participants.Any(x => x.Id == id))
                throw new BadRequestException("User is already in correspondence");
            
            participants.Add(_applicationContext.Users.Get(id));
        }
        participants.Add(sender);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MessageAddModel, Message>();

            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<Message, MessageViewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Correspondence correspondence = new Correspondence
        {
            Users = participants,
            Name = sender.FirstName
        };
        _applicationContext.Correspondences.Add(correspondence);

        Message message = mapper.Map<MessageAddModel, Message>(messageAddModel);
        message.Sender = sender;
        message.CorrespondenceId = correspondence.Id;
        _applicationContext.Messages.Add(message);

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel SendMessage(MessageAddModel messageAddModel, int correspondenceId, int senderId)
    {
        User sender = _applicationContext.Users.Get(senderId);
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);
        
        if (!correspondence.Users.Any(x => x.Id == senderId))
            throw new NotFoundException("Correspondence not found");

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MessageAddModel, Message>();

            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<Message, MessageViewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Message message = mapper.Map<MessageAddModel, Message>(messageAddModel);
        message.Sender = sender;
        message.CorrespondenceId = correspondenceId;
        _applicationContext.Messages.Add(message);

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel EditMessage(MessageUpdateModel messageUpdateModel, int senderId)
    {
        Message message = _applicationContext.Messages.Get(messageUpdateModel.Id);
        
        if (message.Sender.Id != senderId) 
            throw new NotFoundException("Message not found");
            
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MessageUpdateModel, Message>();

            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<Message, MessageViewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Message updateMessage = mapper.Map<MessageUpdateModel, Message>(messageUpdateModel);
        updateMessage.IsEdited = true;
        _applicationContext.Messages.Update(updateMessage);

        Correspondence correspondence = _applicationContext.Correspondences.Get(message.CorrespondenceId);

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel DeleteMessage(long messageId, int correspondenceId, int senderId)
    {
        Message message = _applicationContext.Messages.Get(messageId);

        if (message.Sender.Id != senderId)
            throw new NotFoundException("Message not found");
        
        _applicationContext.Messages.Delete(messageId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<Message, MessageViewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Correspondence correspondence;
        try
        {
            correspondence = _applicationContext.Correspondences.Get(correspondenceId);
        }
        catch
        {
            throw new NotFoundException("Correspondence not found");
        }

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return correspondencePreviewModel;
    }
}