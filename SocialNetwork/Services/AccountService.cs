using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Exceptions;
using SocialNetwork.Models;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Images;
using SocialNetwork.Models.Roles;
using SocialNetwork.Models.Users;
using SocialNetwork.Repositories;
using SocialNetwork.Repository;

namespace SocialNetwork.Services;

public class AccountService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IOptions<JwtAuthenticationOptions> _jwtAuthOptions;

    public AccountService(IApplicationContext applicationContext, IOptions<JwtAuthenticationOptions> jwtAuthOptions)
    {
        _applicationContext = applicationContext;
        _jwtAuthOptions = jwtAuthOptions;
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

    public IEnumerable<FriendsRequestViewModel> GetFriendRequests(int userId)
    {
        IEnumerable<FriendRequest> friendRequests = _applicationContext.FriendRequests.GetUserFriendsRequests(userId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FriendRequest, FriendsRequestViewModel>();
            cfg.CreateMap<User, UserPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        IEnumerable<FriendsRequestViewModel> friendsRequestViewModels =
            mapper.Map<IEnumerable<FriendRequest>, IEnumerable<FriendsRequestViewModel>>(friendRequests);
        return friendsRequestViewModels;
    }

    public UserPreviewModel ConfirmFriendRequest(int friendRequestId, int userId)
    {
        FriendRequest friendRequest = _applicationContext.FriendRequests.Get(friendRequestId);
        if (friendRequest.RecipientId != userId)
            throw new NotFoundException("Friend request not found");

        _applicationContext.Users.AddUserToFriends(userId, friendRequest.Sender.Id);
        _applicationContext.FriendRequests.Delete(friendRequestId);

        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        User userFriend = _applicationContext.Users.Get(friendRequest.Sender.Id);
        UserPreviewModel userPreviewModel = mapper.Map<User, UserPreviewModel>(userFriend);
        return userPreviewModel;
    }

    public ImageViewModel AddPhotoToAccount(ImageAddModel imageAddModel, int userId)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ImageAddModel, Image>().ForMember(nameof(Image.ImageData), opt =>
                opt.MapFrom(x => Convert.FromBase64String(x.ImageData)));

            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        });
        var mapper = new Mapper(mapperConfig);

        Image image;
        try
        {
            image = mapper.Map<ImageAddModel, Image>(imageAddModel);
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }

        image.UserId = userId;
        _applicationContext.Images.Add(image);
        _applicationContext.Images.AddPhotoToUser(userId, image.Id);

        ImageViewModel imageViewModel = mapper.Map<Image, ImageViewModel>(image);
        return imageViewModel;
    }

    public ImageViewModel DeletePhotoFromAccount(int imageId, int userId)
    {
        Image image = _applicationContext.Images.Get(imageId);
        
        if (image.UserId != userId)
            throw new NotFoundException("Image not found");
        
        _applicationContext.Images.DeletePhotoFromUser(userId, imageId);
        _applicationContext.Images.Delete(imageId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        });
        var mapper = new Mapper(mapperConfig);

        ImageViewModel imageViewModel = mapper.Map<Image, ImageViewModel>(image);
        return imageViewModel;
    }

    public ImageViewModel ChangeAvatar(int imageId, int userId)
    {
        Image image = _applicationContext.Images.Get(imageId);
        
        if(image.UserId != userId)
            throw new NotFoundException("Image not found");
        
        _applicationContext.Images.UpdateUserAvatar(imageId, userId);
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        });
        var mapper = new Mapper(mapperConfig);

        ImageViewModel imageViewModel = mapper.Map<Image, ImageViewModel>(image);
        return imageViewModel;
    }

    public string Register(RegisterUserModel registerUserModel)
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<RegisterUserModel, User>());
        var mapper = new Mapper(mapperConfig);

        User user = mapper.Map<RegisterUserModel, User>(registerUserModel);
        user.PasswordHash = PasswordHasher.HashPassword(registerUserModel.Password);
        try
        {
            _applicationContext.Users.Add(user);
        }
        catch
        {
            throw new BadRequestException("User with the same email already exists");
        }
        
        _applicationContext.Images.SetDefaultValueForUserAvatar(user.Id);
        
        return GenerateJwt(user);
    }

    public string Login(LoginUserModel loginUserModel)
    {
        User user = _applicationContext.Users.GetFromEmail(loginUserModel.Email);
        if (!PasswordHasher.VerifyHashedPassword(user.PasswordHash, loginUserModel.Password))
        {
            throw new BadRequestException("Incorrect login or password");
        }

        return GenerateJwt(user);
    }

    private string GenerateJwt(User user)
    {
        JwtAuthenticationOptions jwtAuthenticationOptions = _jwtAuthOptions.Value;

        SymmetricSecurityKey securityKey = jwtAuthenticationOptions.GetSymmetricSecurityKey();
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        };

        IEnumerable<Role> roles = _applicationContext.Roles.GetFromUserId(user.Id).ToList();
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        var token = new JwtSecurityToken(
            jwtAuthenticationOptions.Issuer,
            jwtAuthenticationOptions.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(jwtAuthenticationOptions.TokenMinuteLifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}