﻿using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Roles;
using SocialNetwork.Repositories;

namespace SocialNetwork.Services;

public class RolesService
{
    private readonly IApplicationContext _applicationContext;

    public RolesService(IApplicationContext applicationContext)
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

    public RoleViewModel Update(RoleEditModel roleEditModel)
    {
        _applicationContext.Roles.Get(roleEditModel.Id);
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<RoleEditModel, Role>();
            cfg.CreateMap<Role, RoleViewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Role role = mapper.Map<RoleEditModel, Role>(roleEditModel);
        _applicationContext.Roles.Update(role);

        return mapper.Map<Role, RoleViewModel>(role);
    }

    public RoleViewModel Delete(int roleId)
    {
        Role role = _applicationContext.Roles.Get(roleId);
        
        _applicationContext.Roles.Delete(roleId);
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Role, RoleViewModel>());
        var mapper = new Mapper(mapperConfig);

        return mapper.Map<Role, RoleViewModel>(role);
    }
}