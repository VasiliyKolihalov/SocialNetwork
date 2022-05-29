using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialNetwork.Exceptions;
using SocialNetwork.Models;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Roles;
using SocialNetwork.Models.Users;
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

        });
        var mapper = new Mapper(mapperConfig);

        UserViewModel userViewModel = mapper.Map<User, UserViewModel>(user);
        IEnumerable<Community> communities = _applicationContext.Communities.GetFollowedCommunity(userId);
        IEnumerable<User> friends = _applicationContext.Users.GetUserFriends(userId);
        
        userViewModel.Communities = mapper.Map<IEnumerable<Community>, List<CommunityPreviewModel>>(communities);
        userViewModel.Friends = mapper.Map<IEnumerable<User>, List<UserPreviewModel>>(friends);
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

        List<Role> roles = _applicationContext.Roles.GetFromUserId(user.Id).ToList();
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