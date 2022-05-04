using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Models.Users;
using SocialNetwork.Services;

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
    
    private int GetUserId()
    {
        int userId = Convert.ToInt32(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
        return userId;
    } 
    
    [Authorize]
    [HttpGet]
    public ActionResult<UserViewModel> Get()
    {
        UserViewModel userViewModel = _accountService.Get(GetUserId());
        return Ok(userViewModel);
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