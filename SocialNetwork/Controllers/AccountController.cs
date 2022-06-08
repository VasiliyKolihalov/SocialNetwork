using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Users;
using SocialNetwork.Services;
using SocialNetwork.Extensions;
using SocialNetwork.Models.Images;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    [Authorize]
    [HttpGet]
    public ActionResult<UserViewModel> Get()
    {
        UserViewModel userViewModel = _accountService.Get(this.GetUserIdFromClaims());
        return Ok(userViewModel);
    }
    
    [Authorize]
    [Route("GetFriendRequests")]
    [HttpGet]
    public ActionResult<IEnumerable<FriendsRequestViewModel>> GetFriendRequests()
    {
        IEnumerable<FriendsRequestViewModel> friendsRequestViewModels = _accountService.GetFriendRequests(this.GetUserIdFromClaims());
        return Ok(friendsRequestViewModels);
    }

    [Authorize]
    [Route("ConfirmFriendRequest/{friendRequestId}")]
    [HttpPost]
    public ActionResult<UserPreviewModel> ConfirmFriendRequest(int friendRequestId)
    {
        UserPreviewModel userPreviewModel = _accountService.ConfirmFriendRequest(friendRequestId, this.GetUserIdFromClaims());
        return Ok(userPreviewModel);
    }

    [Authorize]
    [Route("AddPhotoToAccount")]
    [HttpPost]
    public ActionResult<ImageViewModel> AddPhotoToAccount(ImageAddModel imageAddModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        ImageViewModel imageViewModel = _accountService.AddPhotoToAccount(imageAddModel, this.GetUserIdFromClaims());
        return Ok(imageViewModel);
    }

    [Authorize]
    [Route("DeletePhotoFromAccount/{imageId}")]
    [HttpDelete]
    public ActionResult<ImageViewModel> DeletePhotoFromAccount(int imageId)
    {
        ImageViewModel imageViewModel = _accountService.DeletePhotoFromAccount(imageId, this.GetUserIdFromClaims());
        return Ok(imageViewModel);
    }

    [Authorize]
    [Route("ChangeAvatar/{imageId}")]
    [HttpPut]
    public ActionResult<ImageViewModel> ChangeAvatar(int imageId)
    {
        ImageViewModel imageViewModel = _accountService.ChangeAvatar(imageId, this.GetUserIdFromClaims());
        return Ok(imageViewModel);
    }

    [Route("Register")]
    [HttpPost]
    public ActionResult Register(RegisterUserModel registerUserModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        string token = _accountService.Register(registerUserModel);
        return Ok(new {Token = token});
    }

    [Route("Login")]
    [HttpPost]
    public ActionResult Login(LoginUserModel loginUserModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        string token = _accountService.Login(loginUserModel);
        return Ok(new {Token = token});
    }
    
}