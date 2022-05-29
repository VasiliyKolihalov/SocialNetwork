using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Users;
using SocialNetwork.Repository;

namespace SocialNetwork.Services;

public class UsersService
{
    private readonly IApplicationContext _applicationContext;

    public UsersService(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public IEnumerable<UserPreviewModel> GetAll()
    {
        IEnumerable<User> users = _applicationContext.Users.GetAll();

        var mapperConfig = new MapperConfiguration(cfg =>
            cfg.CreateMap<User, UserPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        IEnumerable<UserPreviewModel> userViewModels =
            mapper.Map<IEnumerable<User>, IEnumerable<UserPreviewModel>>(users);
        return userViewModels;
    }

    public UserViewModel Get(int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserViewModel>();
            
            cfg.CreateMap<User, UserPreviewModel>();
            cfg.CreateMap<Community, CommunityPreviewModel>();

        });
        var mapper = new Mapper(mapperConfig);

        UserViewModel userViewModel = mapper.Map<User, UserViewModel>(user);
        IEnumerable<Community> communities = _applicationContext.Communities.GetFollowedCommunity(userId);
        IEnumerable<User> friends = _applicationContext.Users.GetUserFriends(userId);
        
        userViewModel.Communities = mapper.Map<IEnumerable<Community>, List<CommunityPreviewModel>>(communities);
        userViewModel.Friends = mapper.Map<IEnumerable<User>, List<UserPreviewModel>>(friends);
        return userViewModel;
    }

    public UserPreviewModel SendFriendRequest(FriendRequestAddModel friendRequestAddModel, int senderId)
    {
        if (friendRequestAddModel.RecipientId == senderId)
            throw new BadRequestException("Sender and recipient are same");

        User recipientUser = _applicationContext.Users.Get(friendRequestAddModel.RecipientId);

        IEnumerable<FriendRequest> userRecipientFriendRequests = _applicationContext.FriendRequests.GetUserFriendsRequests(friendRequestAddModel.RecipientId);
        if (userRecipientFriendRequests.Any(x => x.RecipientId == friendRequestAddModel.RecipientId && x.Sender.Id == senderId))
            throw new BadRequestException("Request has already been sent");

        IEnumerable<User> userRecipientFriends = _applicationContext.Users.GetUserFriends(friendRequestAddModel.RecipientId);
        if (userRecipientFriends.Any(x => x.Id == senderId))
            throw new BadRequestException("User is already friends");

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FriendRequestAddModel, FriendRequest>();

            cfg.CreateMap<User, UserPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        FriendRequest friendRequest = mapper.Map<FriendRequestAddModel, FriendRequest>(friendRequestAddModel);
        User senderUser = _applicationContext.Users.Get(senderId);
        friendRequest.Sender = senderUser;
        friendRequest.RecipientId = friendRequestAddModel.RecipientId;
        _applicationContext.FriendRequests.Add(friendRequest);

        UserPreviewModel userPreviewModel = mapper.Map<User, UserPreviewModel>(recipientUser);
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
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        UserPreviewModel userPreviewModel = mapper.Map<User, UserPreviewModel>(userFriend);
        return userPreviewModel;
    }

    public UserPreviewModel Update(UserPutModel userPutModel)
    {
        _applicationContext.Users.Get(userPutModel.Id);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserPutModel, User>();
            cfg.CreateMap<User, UserPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        User user = mapper.Map<UserPutModel, User>(userPutModel);
        _applicationContext.Users.Update(user);

        UserPreviewModel userPreviewModel = mapper.Map<User, UserPreviewModel>(user);
        return userPreviewModel;
    }

    public UserPreviewModel Delete(int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        _applicationContext.Users.Delete(userId);

        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        UserPreviewModel userPreviewModel = mapper.Map<User, UserPreviewModel>(user);
        return userPreviewModel;
    }
}