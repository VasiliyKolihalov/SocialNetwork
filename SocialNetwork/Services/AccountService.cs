﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

    public AccountService(IApplicationContext applicationContext, IOptions<JwtAuthenticationOptions> jwtAuthOptions, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _jwtAuthOptions = jwtAuthOptions;
        _mapper = mapper;
    }

    public IEnumerable<FriendsRequestViewModel> GetFriendRequests(int userId)
    {
        IEnumerable<FriendRequest> friendRequests = _applicationContext.FriendRequests.GetUserFriendsRequests(userId);
        
        IEnumerable<FriendsRequestViewModel> friendsRequestViewModels = _mapper.Map<IEnumerable<FriendsRequestViewModel>>(friendRequests);
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

        user.PasswordHash = PasswordHasher.HashPassword(changeModel.NewPassword);
        _applicationContext.Users.ChangePasswordHash(user.Id, user.PasswordHash);

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