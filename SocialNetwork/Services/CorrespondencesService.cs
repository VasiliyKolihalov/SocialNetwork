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
        Correspondence correspondence;
        try
        {
            correspondence = _applicationContext.Correspondences.Get(correspondenceId);
            if (correspondence.Users.FirstOrDefault(x => x.Id == userId) == null)
            {
                throw new Exception();
            }
        }
        catch
        {
            throw new Exception("Correspondence not found");
        }

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
                _applicationContext.Messages.GetBasedCorrespondence(correspondenceId));

        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel StartCorrespondence(MessageAddModel messageAddModel, List<int> participantsId,
        int senderId)
    {
        User sender = _applicationContext.Users.Get(senderId);

        var participants = new List<User>();
        foreach (int id in participantsId)
        {
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
        message.Correspondence = correspondence;
        _applicationContext.Messages.Add(message);

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel SendMessage(MessageAddModel messageAddModel, int correspondenceId, int senderId)
    {
        User sender = _applicationContext.Users.Get(senderId);

        Correspondence correspondence;
        try
        {
            correspondence = _applicationContext.Correspondences.Get(correspondenceId);
            if (correspondence.Users.FirstOrDefault(x => x.Id == senderId) == null)
            {
                throw new Exception();
            }
        }
        catch
        {
            throw new NotFoundException("Correspondence not found");
        }

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
        message.Correspondence = correspondence;
        _applicationContext.Messages.Add(message);

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel EditMessage(MessageUpdateModel messageUpdateModel, int senderId)
    {
        Message message;
        try
        {
            message = _applicationContext.Messages.Get(messageUpdateModel.Id);

            if (message.Sender.Id != senderId)
                throw new Exception();
        }
        catch
        {
            throw new NotFoundException("Message not found");
        }

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

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(message.Correspondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel DeleteMessage(long messageId, int correspondenceId, int senderId)
    {
        Message message = _applicationContext.Messages.Get(messageId);
        
        if (message.Sender.Id != senderId)
        {
            throw new NotFoundException("Message not found");
        }
        
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