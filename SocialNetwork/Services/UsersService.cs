using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Images;
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

        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        IEnumerable<UserPreviewModel> userPreviewModels = mapper.Map<IEnumerable<User>, IEnumerable<UserPreviewModel>>(users);
        
        return userPreviewModels;
    }

    public UserViewModel Get(int userId)
    {
        User user = _applicationContext.Users.Get(userId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserViewModel>();

            cfg.CreateMap<User, UserPreviewModel>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        });
        var mapper = new Mapper(mapperConfig);

        IEnumerable<Community> communities = _applicationContext.Communities.GetFollowedCommunity(userId);
        IEnumerable<User> friends = _applicationContext.Users.GetUserFriends(userId);
        Image? avatar = _applicationContext.Images.GetUserAvatar(userId);
        IEnumerable<Image> photos = _applicationContext.Images.GetUserPhotos(userId);

        UserViewModel userViewModel = mapper.Map<User, UserViewModel>(user);
        userViewModel.Communities = mapper.Map<IEnumerable<Community>, List<CommunityPreviewModel>>(communities);
        userViewModel.Friends = mapper.Map<IEnumerable<User>, List<UserPreviewModel>>(friends);
        userViewModel.Photos = mapper.Map<IEnumerable<Image>, List<ImageViewModel>>(photos);
        userViewModel.Avatar = avatar == null ? null : mapper.Map<Image, ImageViewModel>(avatar);
        
        return userViewModel;
    }
    
    public ImageViewModel? GetUserAvatar(int userId)
    {
        _applicationContext.Users.Get(userId);

        Image? image = _applicationContext.Images.GetUserAvatar(userId);
        
        if(image == null)
            return null;
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        });
        var mapper = new Mapper(mapperConfig);

        ImageViewModel imageViewModel = mapper.Map<Image, ImageViewModel>(image);
        return imageViewModel;
    }

    public UserPreviewModel SendFriendRequest(FriendRequestAddModel friendRequestAddModel, int recipientId, int senderId)
    {
        if (recipientId == senderId)
            throw new BadRequestException("Sender and recipient are same");

        User recipientUser = _applicationContext.Users.Get(recipientId);

        IEnumerable<FriendRequest> userRecipientFriendRequests =
            _applicationContext.FriendRequests.GetUserFriendsRequests(recipientId);
        if (userRecipientFriendRequests.Any(x =>
                x.RecipientId == recipientId && x.Sender.Id == senderId))
            throw new BadRequestException("Request has already been sent");

        IEnumerable<User> userRecipientFriends =
            _applicationContext.Users.GetUserFriends(recipientId);
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
        friendRequest.RecipientId = recipientId;
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

    public UserPreviewModel Update(UserEditModel userEditModel)
    {
        _applicationContext.Users.Get(userEditModel.Id);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserEditModel, User>();
            cfg.CreateMap<User, UserPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        User user = mapper.Map<UserEditModel, User>(userEditModel);
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