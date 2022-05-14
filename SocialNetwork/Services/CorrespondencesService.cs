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
            throw new NotFoundException("Correspondence not found");

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

    public CorrespondencePreviewModel StartCorrespondence(CorrespondenceAddModel correspondenceAddModel, int senderId)
    {
        User sender = _applicationContext.Users.Get(senderId);

        var participants = new List<User>();
        foreach (int id in correspondenceAddModel.ParticipantsId)
        {
            if (participants.Any(x => x.Id == id))
                throw new BadRequestException("User repeats");

            participants.Add(_applicationContext.Users.Get(id));
        }

        participants.Add(sender);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CorrespondenceAddModel, Correspondence>();
            cfg.CreateMap<MessageAddModel, Message>();

            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Correspondence correspondence = mapper.Map<CorrespondenceAddModel, Correspondence>(correspondenceAddModel);
        correspondence.Admin = sender;
        correspondence.Users = participants;

        _applicationContext.Correspondences.Add(correspondence);

        Message message = mapper.Map<MessageAddModel, Message>(correspondenceAddModel.MessageAddModel);
        message.Sender = sender;
        message.CorrespondenceId = correspondence.Id;
        _applicationContext.Messages.Add(message);

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel AddUserToCorrespondence(int correspondenceId, int addedUserId, int adminId)
    {
        _applicationContext.Users.Get(addedUserId);
        _applicationContext.Users.Get(adminId);
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);

        if (correspondence.Admin.Id != adminId)
            throw new BadRequestException("User is not correspondence admin");

        if (correspondence.Users.Any(x => x.Id == addedUserId))
            throw new BadRequestException("User is already in correspondence");

        _applicationContext.Correspondences.AddUserToCorrespondence(addedUserId, correspondenceId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        CorrespondencePreviewModel communityPreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return communityPreviewModel;
    }

    public CorrespondencePreviewModel DeleteUserFromCorrespondence(int correspondenceId, int addedUserId, int adminId)
    {
        _applicationContext.Users.Get(addedUserId);
        _applicationContext.Users.Get(adminId);
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);

        if (correspondence.Admin.Id != adminId)
            throw new BadRequestException("User is not correspondence admin");

        if (!correspondence.Users.Any(x => x.Id == addedUserId))
            throw new BadRequestException("User not in correspondence");

        _applicationContext.Correspondences.DeleteUserFromCorrespondence(addedUserId, correspondenceId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        CorrespondencePreviewModel communityPreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return communityPreviewModel;
    }

    public CorrespondencePreviewModel Edit(CorrespondenceEditModel correspondenceEditModel, int adminId)
    {
        _applicationContext.Users.Get(adminId);
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceEditModel.Id);

        if (correspondence.Admin.Id != adminId)
            throw new BadRequestException("User is not correspondence admin");

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CorrespondenceEditModel, Correspondence>();

            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Correspondence updatedCorrespondence =
            mapper.Map<CorrespondenceEditModel, Correspondence>(correspondenceEditModel);
        _applicationContext.Correspondences.Update(updatedCorrespondence);

        CorrespondencePreviewModel correspondencePreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(updatedCorrespondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel Delete(int correspondenceId, int adminId)
    {
        _applicationContext.Users.Get(adminId);
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);

        if (correspondence.Admin.Id != adminId)
            throw new BadRequestException("User is not correspondence admin");

        _applicationContext.Correspondences.Delete(correspondenceId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        CorrespondencePreviewModel communityPreviewModel =
            mapper.Map<Correspondence, CorrespondencePreviewModel>(correspondence);
        return communityPreviewModel;
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

    public CorrespondencePreviewModel EditMessage(MessageEditModel messageEditModel, int senderId)
    {
        Message message = _applicationContext.Messages.Get(messageEditModel.Id);

        if (message.Sender.Id != senderId)
            throw new NotFoundException("Message not found");

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MessageEditModel, Message>();

            cfg.CreateMap<Correspondence, CorrespondencePreviewModel>();
            cfg.CreateMap<Message, MessageViewModel>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Message updateMessage = mapper.Map<MessageEditModel, Message>(messageEditModel);
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