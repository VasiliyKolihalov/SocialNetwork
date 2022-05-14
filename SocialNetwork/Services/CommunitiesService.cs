using System.Security.Cryptography;
using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Posts;
using SocialNetwork.Models.Users;
using SocialNetwork.Repository;

namespace SocialNetwork.Services;

public class CommunitiesService
{
    private readonly ApplicationContext _applicationContext;

    public CommunitiesService(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public IEnumerable<CommunityPreviewModel> GetAll()
    {
        IEnumerable<Community> communities = _applicationContext.Communities.GetAll();

        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Community, CommunityPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        IEnumerable<CommunityPreviewModel> communityPreviewModels =
            mapper.Map<IEnumerable<Community>, IEnumerable<CommunityPreviewModel>>(communities);

        return communityPreviewModels;
    }

    public IEnumerable<CommunityPreviewModel> GetFollowed(int userId)
    {
        _applicationContext.Users.Get(userId);

        IEnumerable<Community> communities = _applicationContext.Communities.GetFollowedCommunity(userId);

        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Community, CommunityPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        IEnumerable<CommunityPreviewModel> communityPreviewModels =
            mapper.Map<IEnumerable<Community>, IEnumerable<CommunityPreviewModel>>(communities);

        return communityPreviewModels;
    }

    public IEnumerable<CommunityPreviewModel> GetManaged(int userId)
    {
        _applicationContext.Users.Get(userId);

        IEnumerable<Community> communities = _applicationContext.Communities.GetManagedCommunity(userId);

        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Community, CommunityPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        IEnumerable<CommunityPreviewModel> communityPreviewModels =
            mapper.Map<IEnumerable<Community>, IEnumerable<CommunityPreviewModel>>(communities);

        return communityPreviewModels;
    }

    public CommunityViewModel GetWithPosts(int communityId)
    {
        Community community = _applicationContext.Communities.Get(communityId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Community, CommunityViewModel>();
            cfg.CreateMap<User, UserViewModel>();
            cfg.CreateMap<Post, PostViewModel>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        IEnumerable<PostViewModel> posts = mapper.Map<IEnumerable<Post>, IEnumerable<PostViewModel>>(_applicationContext.Posts.GetPostsFromCommunity(communityId));
        foreach (var post in posts)
        {
            post.Community = mapper.Map<Community, CommunityPreviewModel>(community);
        }
        
        CommunityViewModel communityViewModel = mapper.Map<Community, CommunityViewModel>(community);
        communityViewModel.Posts = posts.ToList();

        return communityViewModel;
    }

    public CommunityPreviewModel Subscribe(int communityId, int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        Community community = _applicationContext.Communities.Get(communityId);

        if (community.Users.Any(x => x.Id == userId))
            throw new BadRequestException("User is already in community");
        
        _applicationContext.Communities.SubscribeUserToCommunity(communityId, userId);
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CommunityAddModel, Community>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    public CommunityPreviewModel Unsubscribe(int communityId, int userId)
    {
        User user = _applicationContext.Users.Get(userId);
        Community community = _applicationContext.Communities.Get(communityId);
        
        if (!community.Users.Any(x => x.Id == userId))
            throw new BadRequestException("User is not member of community");
        
        _applicationContext.Communities.UnsubscribeUserFromCommunity(communityId, userId);
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CommunityAddModel, Community>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    public CommunityPreviewModel Create(CommunityAddModel communityAddModel, int userId)
    {
        User author = _applicationContext.Users.Get(userId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CommunityAddModel, Community>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Community community = mapper.Map<CommunityAddModel, Community>(communityAddModel);
        community.Author = author;
        
        _applicationContext.Communities.Add(community);

        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    public CommunityPreviewModel Edit(CommunityEditModel communityEditModel, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityEditModel.Id);
        _applicationContext.Users.Get(userId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CommunityEditModel, Community>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Community updatedCommunity = mapper.Map<CommunityEditModel, Community>(communityEditModel);
        _applicationContext.Communities.Update(updatedCommunity);

        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(updatedCommunity);
        return communityPreviewModel;
    }

    public CommunityPreviewModel Delete(int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId); 
        _applicationContext.Users.Get(userId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        _applicationContext.Communities.Delete(communityId);
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Community, CommunityPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    public CommunityPreviewModel AddPost(PostAddModel postAddModel, int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);
        _applicationContext.Users.Get(userId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PostAddModel, Post>();
            
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Post post = mapper.Map<PostAddModel, Post>(postAddModel);
        post.CommunityId = communityId;
        
        _applicationContext.Posts.Add(post);
        
        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    public CommunityPreviewModel EditPost(PostEditModel postEditModel, int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);
        _applicationContext.Users.Get(userId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PostEditModel, Post>();
            
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        Post post = mapper.Map<PostEditModel, Post>(postEditModel);
        _applicationContext.Posts.Update(post);
        
        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    public CommunityPreviewModel DeletePost(int postId, int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);
         _applicationContext.Users.Get(userId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");

        _applicationContext.Posts.Get(postId);
        _applicationContext.Posts.Delete(postId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PostAddModel, Post>();
            
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(community);
        return communityPreviewModel;
    }
}