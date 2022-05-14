using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Users;
using SocialNetwork.Repository;

namespace SocialNetwork.Services;

public class UsersService
{
    private readonly ApplicationContext _applicationContext;

    public UsersService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public IEnumerable<UserViewModel> GetAll()
    {
        IEnumerable<User> users = _applicationContext.Users.GetAll();
        
        var mapperConfig = new MapperConfiguration(cfg =>
            cfg.CreateMap<User, UserViewModel>());
        var mapper = new Mapper(mapperConfig);

        IEnumerable<UserViewModel> userViewModels = mapper.Map<IEnumerable<User>, IEnumerable<UserViewModel>>(users);
        return userViewModels;
    }

    public UserViewModel Get(int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserViewModel>());
        var mapper = new Mapper(mapperConfig);

        UserViewModel userViewModel = mapper.Map<User, UserViewModel>(user);
        return userViewModel;
    }
    
    public UserViewModel Update(UserPutModel userPutModel)
    {
        _applicationContext.Users.Get(userPutModel.Id);
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserPutModel, User>();
            cfg.CreateMap<User, UserViewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        User user = mapper.Map<UserPutModel, User>(userPutModel);
        _applicationContext.Users.Update(user);

        UserViewModel userViewModel = mapper.Map<User, UserViewModel>(user);
        return userViewModel;
    }

    public UserViewModel Delete(int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        _applicationContext.Users.Delete(userId);
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserViewModel>());
        var mapper = new Mapper(mapperConfig);

        UserViewModel userViewModel = mapper.Map<User, UserViewModel>(user);
        return userViewModel;
    }
}