using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Correspondences;
using SocialNetwork.Models.Messages;
using SocialNetwork.Models.Users;
using SocialNetwork.Repositories;

namespace SocialNetwork.Services;

public class CorrespondencesService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMapper _mapper;

    public CorrespondencesService(IApplicationContext applicationContext, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _mapper = mapper;
    }

    public IEnumerable<CorrespondencePreviewModel> GetAll(int userId)
    {
        IEnumerable<Correspondence> correspondences =
            _applicationContext.Correspondences.GetUserCorrespondences(userId);

        IEnumerable<CorrespondencePreviewModel> correspondenceViewModels = _mapper.Map<IEnumerable<CorrespondencePreviewModel>>(correspondences);
        return correspondenceViewModels;
    }

    public CorrespondenceViewModel GetWithMessages(int correspondenceId, int userId)
    {
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);

        if (!correspondence.Users.Any(x => x.Id == userId))
            throw new NotFoundException("Correspondence not found");
        
        CorrespondenceViewModel correspondencePreviewModel = _mapper.Map<CorrespondenceViewModel>(correspondence);
        IEnumerable<Message> messages = _applicationContext.Messages.GetFromCorrespondence(correspondenceId);
        correspondencePreviewModel.Messages = _mapper.Map<List<MessageViewModel>>(messages);

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
        
        Correspondence correspondence = _mapper.Map<Correspondence>(correspondenceAddModel);
        correspondence.Admin = sender;
        correspondence.Users = participants;

        _applicationContext.Correspondences.Add(correspondence);

        Message message = _mapper.Map<Message>(correspondenceAddModel.MessageAddModel);
        message.Sender = sender;
        message.CorrespondenceId = correspondence.Id;
        _applicationContext.Messages.Add(message);

        CorrespondencePreviewModel correspondencePreviewModel = _mapper.Map<CorrespondencePreviewModel>(correspondence);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel AddUserToCorrespondence(int correspondenceId, int userId, int senderId)
    {
        _applicationContext.Users.Get(userId);
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);

        if (correspondence.Admin.Id != senderId)
            throw new BadRequestException("User is not correspondence admin");

        if (correspondence.Users.Any(x => x.Id == userId))
            throw new BadRequestException("User is already in correspondence");

        _applicationContext.Correspondences.AddUserToCorrespondence(userId, correspondenceId);

        Correspondence updatedCorrespondence = _applicationContext.Correspondences.Get(correspondenceId);
        CorrespondencePreviewModel communityPreviewModel = _mapper.Map<CorrespondencePreviewModel>(updatedCorrespondence);
        return communityPreviewModel;
    }

    public CorrespondencePreviewModel DeleteUserFromCorrespondence(int correspondenceId, int userId, int senderId)
    {
        _applicationContext.Users.Get(userId);
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);

        if (correspondence.Admin.Id != senderId)
            throw new BadRequestException("User is not correspondence admin");

        if (!correspondence.Users.Any(x => x.Id == userId))
            throw new BadRequestException("User not in correspondence");

        _applicationContext.Correspondences.DeleteUserFromCorrespondence(userId, correspondenceId);

        Correspondence updatedCorrespondence = _applicationContext.Correspondences.Get(correspondenceId);
        CorrespondencePreviewModel communityPreviewModel = _mapper.Map<CorrespondencePreviewModel>(updatedCorrespondence);
        return communityPreviewModel;
    }

    public CorrespondencePreviewModel Edit(CorrespondenceEditModel correspondenceEditModel, int senderId)
    {
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceEditModel.Id);

        if (correspondence.Admin.Id != senderId)
            throw new BadRequestException("User is not correspondence admin");
        
        Correspondence updatedCorrespondence = _mapper.Map<Correspondence>(correspondenceEditModel);
        _applicationContext.Correspondences.Update(updatedCorrespondence);

        CorrespondencePreviewModel correspondencePreviewModel = _mapper.Map<CorrespondencePreviewModel>(updatedCorrespondence);
        correspondencePreviewModel.Users = _mapper.Map<List<UserPreviewModel>>(correspondence.Users);
        return correspondencePreviewModel;
    }

    public CorrespondencePreviewModel Delete(int correspondenceId, int senderId)
    {
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);

        if (correspondence.Admin.Id != senderId)
            throw new BadRequestException("User is not correspondence admin");

        _applicationContext.Correspondences.Delete(correspondenceId);
        
        CorrespondencePreviewModel communityPreviewModel = _mapper.Map<CorrespondencePreviewModel>(correspondence);
        return communityPreviewModel;
    }

    public MessageViewModel SendMessage(MessageAddModel messageAddModel, int correspondenceId, int senderId)
    {
        User sender = _applicationContext.Users.Get(senderId);
        Correspondence correspondence = _applicationContext.Correspondences.Get(correspondenceId);

        if (!correspondence.Users.Any(x => x.Id == senderId))
            throw new NotFoundException("Correspondence not found");
        
        Message message = _mapper.Map<Message>(messageAddModel);
        message.Sender = sender;
        message.CorrespondenceId = correspondenceId;
        _applicationContext.Messages.Add(message);

        MessageViewModel messageViewModel = _mapper.Map<MessageViewModel>(message);
        return messageViewModel;
    }

    public MessageViewModel EditMessage(MessageEditModel messageEditModel, int senderId)
    {
        Message message = _applicationContext.Messages.Get(messageEditModel.Id);

        if (message.Sender.Id != senderId)
            throw new NotFoundException("Message not found");
        
        Message updatedMessage = _mapper.Map<Message>(messageEditModel);
        updatedMessage.IsEdited = true;
        _applicationContext.Messages.Update(updatedMessage);

        MessageViewModel messageViewModel = _mapper.Map<MessageViewModel>(updatedMessage);
        messageViewModel.DateTime = message.DateTime;
        messageViewModel.Sender = _mapper.Map<UserPreviewModel>(message.Sender);
        return messageViewModel;
    }

    public MessageViewModel DeleteMessage(long messageId, int senderId)
    {
        Message message = _applicationContext.Messages.Get(messageId);

        if (message.Sender.Id != senderId)
            throw new NotFoundException("Message not found");

        _applicationContext.Messages.Delete(messageId);

        MessageViewModel messageViewModel = _mapper.Map<MessageViewModel>(message);
        return messageViewModel;
    }
}