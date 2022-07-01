using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Roles;
using SocialNetwork.Repositories;

namespace SocialNetwork.Services;

public class RolesService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMapper _mapper;

    public RolesService(IApplicationContext applicationContext, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _mapper = mapper;
    }

    public IEnumerable<RoleViewModel> GetAll()
    {
        IEnumerable<Role> roles = _applicationContext.Roles.GetAll();
        
        return _mapper.Map<IEnumerable<RoleViewModel>>(roles);
    }

    public RoleViewModel Get(int id)
    {
        Role role = _applicationContext.Roles.Get(id);
        
        return _mapper.Map<RoleViewModel>(role);
    }

    public RoleViewModel Create(RoleAddModel roleAddModel)
    {
        try
        {
            Role role = _mapper.Map<Role>(roleAddModel);
            _applicationContext.Roles.Add(role);
            
            return _mapper.Map<RoleViewModel>(role);
        }
        catch
        {
            throw new BadRequestException("Role already exists");
        }
    }

    public RoleViewModel Update(RoleEditModel roleEditModel)
    {
        _applicationContext.Roles.Get(roleEditModel.Id);

        Role role = _mapper.Map<Role>(roleEditModel);
        _applicationContext.Roles.Update(role);

        return _mapper.Map<RoleViewModel>(role);
    }

    public RoleViewModel Delete(int roleId)
    {
        Role role = _applicationContext.Roles.Get(roleId);
        
        _applicationContext.Roles.Delete(roleId);
        
        return _mapper.Map<RoleViewModel>(role);
    }
}