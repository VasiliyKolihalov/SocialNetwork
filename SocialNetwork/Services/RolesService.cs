using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Roles;
using SocialNetwork.Repository;

namespace SocialNetwork.Services;

public class RolesService
{
    private readonly ApplicationContext _applicationContext;

    public RolesService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public IEnumerable<RoleViewModel> GetAll()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Role, RoleViewModel>());
        var mapper = new Mapper(mapperConfig);

        return mapper.Map<IEnumerable<Role>, IEnumerable<RoleViewModel>>(_applicationContext.Roles.GetAll());
    }

    public RoleViewModel Get(int id)
    {
        Role role = _applicationContext.Roles.Get(id);

        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Role, RoleViewModel>());
        var mapper = new Mapper(mapperConfig);

        return mapper.Map<Role, RoleViewModel>(role);
    }

    public RoleViewModel Create(RoleAddModel roleAddModel)
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<RoleAddModel, Role>();
            cfg.CreateMap<Role, RoleViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        try
        {
            Role role = mapper.Map<RoleAddModel, Role>(roleAddModel);
            _applicationContext.Roles.Add(role);
            
            return mapper.Map<Role, RoleViewModel>(role);
        }
        catch
        {
            throw new BadRequestException("Role already exists");
        }
    }

    public RoleViewModel Update(RoleUpdateModel roleUpdateModel)
    {
        _applicationContext.Roles.Get(roleUpdateModel.Id);
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<RoleUpdateModel, Role>();
            cfg.CreateMap<Role, RoleViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Role role = mapper.Map<RoleUpdateModel, Role>(roleUpdateModel);
        _applicationContext.Roles.Update(role);

        return mapper.Map<Role, RoleViewModel>(role);
    }

    public RoleViewModel Delete(int roleId)
    {
        Role deletedRole = _applicationContext.Roles.Get(roleId);
        
        _applicationContext.Roles.Delete(roleId);
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Role, RoleViewModel>());
        var mapper = new Mapper(mapperConfig);

        return mapper.Map<Role, RoleViewModel>(deletedRole);
    }
}