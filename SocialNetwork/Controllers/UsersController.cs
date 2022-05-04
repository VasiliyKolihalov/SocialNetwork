using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Constants;
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
    public ActionResult<IEnumerable<UserViewModel>> GetAll()
    {
        IEnumerable<UserViewModel> userViewModels = _usersService.GetAll();
        return Ok(userViewModels);
    }
    
    [Route("{userId}")]
    [HttpGet]
    public ActionResult<UserViewModel> Get(int userId)
    {
        UserViewModel userViewModel = _usersService.Get(userId);
        return Ok(userViewModel);
    }

    [Authorize(Roles = RolesNameConstants.AdminRole)]
    [HttpPut]
    public ActionResult<UserViewModel> Put(UserPutModel userPutModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        UserViewModel userViewModel = _usersService.Update(userPutModel);
        return Ok(userViewModel);
    }

    [Authorize(Roles = RolesNameConstants.AdminRole)]
    [HttpDelete("{userId}")]
    public ActionResult<UserViewModel> Delete(int userId)
    {
        UserViewModel userViewModel = _usersService.Delete(userId);
        return Ok(userViewModel);
    }
}