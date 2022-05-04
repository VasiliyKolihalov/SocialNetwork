using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Constants;
using SocialNetwork.Models.Roles;
using SocialNetwork.Services;

namespace SocialNetwork.Controllers;

[Authorize(Roles = RolesNameConstants.AdminRole)]
[ApiController]
[Route("[controller]")]
public class RolesController : ControllerBase
{
    private readonly RolesService _rolesService;

    public RolesController(RolesService rolesService)
    {
        _rolesService = rolesService;
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<RoleViewModel>> GetAll()
    {
        IEnumerable<RoleViewModel> roleViewModels = _rolesService.GetAll();
        return Ok(roleViewModels);
    }
    
    [HttpGet("{roleId}")]
    public ActionResult<RoleViewModel> Get(int roleId)
    {
       RoleViewModel roleViewModel = _rolesService.Get(roleId);
       return Ok(roleViewModel);
    }
    
    [HttpPost]
    public ActionResult<RoleViewModel> Post(RoleAddModel roleAddModel)
    {
        RoleViewModel roleViewModel = _rolesService.Create(roleAddModel);
        return Ok(roleViewModel);
    }
    
    [HttpPut]
    public ActionResult<RoleViewModel> Put(RoleUpdateModel roleUpdateModel)
    {
        RoleViewModel roleViewModel = _rolesService.Update(roleUpdateModel);
        return Ok(roleViewModel);
    }
    
    [HttpDelete("{roleId}")]
    public ActionResult<RoleViewModel> Delete(int roleId)
    {
        RoleViewModel roleViewModel = _rolesService.Delete(roleId);
        return Ok(roleViewModel);
    }
}