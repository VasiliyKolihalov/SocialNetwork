using AutoMapper;
using SocialNetwork.Constants;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Images;
using SocialNetwork.Models.Roles;
using SocialNetwork.Models.Users;
using SocialNetwork.Repositories;

namespace SocialNetwork.Services;

public class UsersService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMapper _mapper;

    public UsersService(IApplicationContext applicationContext, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _mapper = mapper;
    }

    public IEnumerable<UserPreviewModel> GetAll()
    {
        IEnumerable<User> users = _applicationContext.Users.GetAll();
        
        IEnumerable<UserPreviewModel> userPreviewModels = _mapper.Map<IEnumerable<UserPreviewModel>>(users);
        
        return userPreviewModels;
    }

    public UserViewModel Get(int userId)
    {
        User user = _applicationContext.Users.Get(userId);

        UserViewModel userViewModel = _mapper.Map<UserViewModel>(user);
        if (user.IsFreeze)
            return userViewModel;

        IEnumerable<Community> communities = _applicationContext.Communities.GetFollowedCommunity(userId);
        IEnumerable<User> friends = _applicationContext.Users.GetUserFriends(userId);
        Image? avatar = _applicationContext.Images.GetUserAvatar(userId);
        IEnumerable<Image> photos = _applicationContext.Images.GetUserPhotos(userId);

        userViewModel.Communities = _mapper.Map<List<CommunityPreviewModel>>(communities);
        userViewModel.Friends = _mapper.Map<List<UserPreviewModel>>(friends);
        userViewModel.Photos = _mapper.Map<List<ImageViewModel>>(photos);
        userViewModel.Avatar = avatar == null ? null : _mapper.Map<ImageViewModel>(avatar);
        
        return userViewModel;
    }
    
    public ImageViewModel? GetUserAvatar(int userId)
    {
        _applicationContext.Users.Get(userId);

        Image? image = _applicationContext.Images.GetUserAvatar(userId);
        
        if(image == null)
            return null;
        
        ImageViewModel imageViewModel = _mapper.Map<ImageViewModel>(image);
        return imageViewModel;
    }

    public UserPreviewModel SendFriendRequest(FriendRequestAddModel friendRequestAddModel, int recipientId, int senderId)
    {
        if (recipientId == senderId)
            throw new BadRequestException("Sender and recipient are same");

        User recipientUser = _applicationContext.Users.Get(recipientId);

        IEnumerable<FriendRequest> userRecipientFriendRequests = _applicationContext.FriendRequests.GetUserFriendsRequests(recipientId);
        if (userRecipientFriendRequests.Any(x => x.RecipientId == recipientId && x.Sender.Id == senderId))
            throw new BadRequestException("Request has already been sent");

        IEnumerable<User> userRecipientFriends = _applicationContext.Users.GetUserFriends(recipientId);
        if (userRecipientFriends.Any(x => x.Id == senderId)) throw new BadRequestException("User is already friends");
        
        FriendRequest friendRequest = _mapper.Map<FriendRequest>(friendRequestAddModel);
        User senderUser = _applicationContext.Users.Get(senderId);
        friendRequest.Sender = senderUser;
        friendRequest.RecipientId = recipientId;
        
        _applicationContext.FriendRequests.Add(friendRequest);

        UserPreviewModel userPreviewModel = _mapper.Map<UserPreviewModel>(recipientUser);
        return userPreviewModel;
    }

    public UserPreviewModel DeleteUserFromFriend(int userId, int senderId)
    {
        if (userId == senderId)
            throw new BadRequestException("Sender and recipient id are same");

        User userFriend = _applicationContext.Users.Get(userId);

        IEnumerable<User> userFriends = _applicationContext.Users.GetUserFriends(userId);
        if (!userFriends.Any(x => x.Id == senderId))
            throw new BadRequestException("User is not in friends");

        _applicationContext.Users.DeleteUserFromFriend(senderId, userId);
        
        UserPreviewModel userPreviewModel = _mapper.Map<UserPreviewModel>(userFriend);
        return userPreviewModel;
    }

    public UserPreviewModel Update(UserEditModel userEditModel, int userId)
    {
        _applicationContext.Users.Get(userEditModel.Id);

        IEnumerable<Role> usersRoles = _applicationContext.Roles.GetFromUserId(userId);
        if (userEditModel.Id != userId && !usersRoles.Any(x => x.Name == RolesNameConstants.AdminRole))
            throw new ForbiddenException("User is not admin");

        User user = _mapper.Map<User>(userEditModel);
        _applicationContext.Users.Update(user);

        UserPreviewModel userPreviewModel = _mapper.Map<UserPreviewModel>(user);
        return userPreviewModel;
    }

    public UserPreviewModel FreezeUser(int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        
        if (user.IsFreeze)
            throw new BadRequestException("User is already freeze");
        
        _applicationContext.Users.FreezeUser(userId);
        user.IsFreeze = true;

        UserPreviewModel userPreviewModel = _mapper.Map<UserPreviewModel>(user);
        return userPreviewModel;
    }

    public UserPreviewModel UnfreezeUser(int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        
        if (!user.IsFreeze)
            throw new BadRequestException("User is not freeze");
        
        _applicationContext.Users.UnfreezeUser(userId);
        user.IsFreeze = false;

        UserPreviewModel userPreviewModel = _mapper.Map<UserPreviewModel>(user);
        return userPreviewModel;
    }
}