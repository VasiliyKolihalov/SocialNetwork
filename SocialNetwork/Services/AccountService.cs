using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scriban;
using SocialNetwork.Exceptions;
using SocialNetwork.Models;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Images;
using SocialNetwork.Models.Roles;
using SocialNetwork.Models.Users;
using SocialNetwork.Repositories;

namespace SocialNetwork.Services;

public class AccountService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMapper _mapper;
    private readonly IOptions<JwtAuthenticationOptions> _jwtAuthOptions;
    private readonly EmailService _emailService;

    public AccountService(IApplicationContext applicationContext, IOptions<JwtAuthenticationOptions> jwtAuthOptions,
        IMapper mapper, EmailService emailService)
    {
        _applicationContext = applicationContext;
        _jwtAuthOptions = jwtAuthOptions;
        _mapper = mapper;
        _emailService = emailService;
    }

    public IEnumerable<FriendsRequestViewModel> GetFriendRequests(int userId)
    {
        IEnumerable<FriendRequest> friendRequests = _applicationContext.FriendRequests.GetUserFriendsRequests(userId);

        IEnumerable<FriendsRequestViewModel> friendsRequestViewModels =
            _mapper.Map<IEnumerable<FriendsRequestViewModel>>(friendRequests);
        return friendsRequestViewModels;
    }

    public UserPreviewModel ConfirmFriendRequest(int friendRequestId, int userId)
    {
        FriendRequest friendRequest = _applicationContext.FriendRequests.Get(friendRequestId);
        if (friendRequest.RecipientId != userId)
            throw new NotFoundException("Friend request not found");

        _applicationContext.Users.AddUserToFriends(userId, friendRequest.Sender.Id);
        _applicationContext.FriendRequests.Delete(friendRequestId);

        User userFriend = _applicationContext.Users.Get(friendRequest.Sender.Id);
        UserPreviewModel userPreviewModel = _mapper.Map<UserPreviewModel>(userFriend);
        return userPreviewModel;
    }

    public ImageViewModel AddPhotoToAccount(ImageAddModel imageAddModel, int userId)
    {
        Image image;
        try
        {
            image = _mapper.Map<Image>(imageAddModel);
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }

        image.UserId = userId;
        _applicationContext.Images.Add(image);
        _applicationContext.Images.AddPhotoToUser(userId, image.Id);

        ImageViewModel imageViewModel = _mapper.Map<ImageViewModel>(image);
        return imageViewModel;
    }

    public ImageViewModel DeletePhotoFromAccount(int imageId, int userId)
    {
        Image image = _applicationContext.Images.Get(imageId);

        if (image.UserId != userId)
            throw new NotFoundException("Image not found");

        _applicationContext.Images.DeletePhotoFromUser(userId, imageId);
        _applicationContext.Images.Delete(imageId);

        ImageViewModel imageViewModel = _mapper.Map<ImageViewModel>(image);
        return imageViewModel;
    }

    public ImageViewModel ChangeAvatar(int imageId, int userId)
    {
        Image image = _applicationContext.Images.Get(imageId);

        if (image.UserId != userId)
            throw new NotFoundException("Image not found");

        _applicationContext.Images.UpdateUserAvatar(imageId, userId);

        ImageViewModel imageViewModel = _mapper.Map<ImageViewModel>(image);
        return imageViewModel;
    }

    public string Register(RegisterUserModel registerUserModel)
    {
        User user = _mapper.Map<User>(registerUserModel);
        ValidatePassword(registerUserModel.Password);
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
            throw new BadRequestException("Incorrect login or password");

        if (user.IsFreeze)
            throw new ForbiddenException("User is freeze");

        return GenerateJwt(user);
    }

    public string ChangePassword(ChangeUserPasswordModel changeModel)
    {
        User user = _applicationContext.Users.GetFromEmail(changeModel.Email);
        
        if (!PasswordHasher.VerifyHashedPassword(user.PasswordHash, changeModel.OldPassword))
            throw new BadRequestException("Incorrect login or password");

        ValidatePassword(changeModel.NewPassword);
        
        user.PasswordHash = PasswordHasher.HashPassword(changeModel.NewPassword);
        _applicationContext.Users.ChangePasswordHash(user.Id, user.PasswordHash);

        return GenerateJwt(user);
    }

    public string GenerateConfirmCode(int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        
        if (user.IsConfirmEmail)
            throw new BadRequestException("User email is already confirm");

        if (_applicationContext.Users.GetEmailConfirmCode(userId) != null)
            throw new BadRequestException("Confirm code is already send");

        Random random = new Random();
        string code = random.Next(1000, 9999).ToString();
        _applicationContext.Users.AddEmailConfirmCode(userId, code);
        return code;
    }
    
    public void SendConfirmCode(int userId, string callbackUrl)
    {
        User user = _applicationContext.Users.Get(userId);
        
        var htmlString = File.ReadAllText("HtmlViews/EmailConfirm.html");
        Template template = Template.Parse(htmlString);
        string message = template.Render(new {callback_url = callbackUrl});

        _emailService.SendEmail(user.Email, "Подтверждение почты", message);
    } 

    public void ConfirmEmail(int userId, string code)
    {
        User user = _applicationContext.Users.Get(userId);

        if (user.IsConfirmEmail)
            throw new BadRequestException("User email is already confirm");
        
        string? confirmCode = _applicationContext.Users.GetEmailConfirmCode(userId);
        
        if (confirmCode == null)
            throw new BadRequestException("User didn't ask for confirm code");

        if (confirmCode != code)
            throw new BadRequestException("Incorrect confirm code");
        
        _applicationContext.Users.DeleteEmailConfirmCode(userId);

        user.IsConfirmEmail = true;
        _applicationContext.Users.ConfirmUserEmail(userId);
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

    private void ValidatePassword(string password)
    {
        if (password.Length < 8 || password.Length > 16)
            throw new BadRequestException("Password length must be between 8 and 16 characters");

        if (!password.Any(char.IsUpper))
            throw new BadRequestException("Password must contain at least 1 capital letter");

        if (!password.Any(char.IsLower))
            throw new BadRequestException("Password must contain at least 1 uppercase letter");

        if (password.Contains(' '))
            throw new BadRequestException("Password must not include spaces");
        
        string specialCharacters = @"%!@#$%^&*()?/>.<,:;'\|}]{[_~`+=-" + "\"";
        if (!specialCharacters.Any(password.Contains))
            throw new BadRequestException("Password must contain special characters");
        
    }
}