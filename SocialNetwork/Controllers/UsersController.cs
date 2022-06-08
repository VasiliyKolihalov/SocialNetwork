using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Constants;
using SocialNetwork.Extensions;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Images;
using SocialNetwork.Models.Users;
using SocialNetwork.Services;

namespace SocialNetwork.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly UsersService _usersService;
    
    public UsersController(UsersService usersService)
    {
        _usersService = usersService;
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<UserPreviewModel>> GetAll()
    {
        IEnumerable<UserPreviewModel> userViewModels = _usersService.GetAll();
        return Ok(userViewModels);
    }
    
    [Route("{userId}")]
    [HttpGet]
    public ActionResult<UserViewModel> Get(int userId)
    {
        UserViewModel userViewModel = _usersService.Get(userId);
        return Ok(userViewModel);
    }

    [Route("{userId}/GetAvatar")]
    [HttpGet]
    public ActionResult<ImageViewModel?> GetUserAvatar(int userId)
    {
        ImageViewModel? imageViewModel = _usersService.GetUserAvatar(userId);
        return Ok(imageViewModel);
    }

    [Authorize]
    [Route("SendFriendRequest")]
    [HttpPost]
    public ActionResult<UserPreviewModel> SendFriendRequest(FriendRequestAddModel friendRequestAddModel)
    {
        UserPreviewModel userPreviewModel = _usersService.SendFriendRequest(friendRequestAddModel, this.GetUserIdFromClaims());
        return Ok(userPreviewModel);
    }

    [Authorize]
    [Route("{userId}/DeleteUserFromFriends")]
    [HttpPost]
    public ActionResult<UserPreviewModel> DeleteUserFromFriends(int userId)
    {
        UserPreviewModel userPreviewModel = _usersService.DeleteUserFromFriend(userId, this.GetUserIdFromClaims());
        return Ok(userPreviewModel);
    }

    [Authorize(Roles = RolesNameConstants.AdminRole)]
    [HttpPut]
    public ActionResult<UserPreviewModel> Put(UserEditModel userEditModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        UserPreviewModel userPreviewModel = _usersService.Update(userEditModel);
        return Ok(userPreviewModel);
    }

    [Authorize(Roles = RolesNameConstants.AdminRole)]
    [HttpDelete("{userId}")]
    public ActionResult<UserPreviewModel> Delete(int userId)
    {
        UserPreviewModel userPreviewModel = _usersService.Delete(userId);
        return Ok(userPreviewModel);
    }
}