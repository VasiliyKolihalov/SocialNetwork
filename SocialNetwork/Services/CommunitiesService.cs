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
    private readonly IApplicationContext _applicationContext;

    public CommunitiesService(IApplicationContext applicationContext)
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
        IEnumerable<Community> communities = _applicationContext.Communities.GetFollowedCommunity(userId);

        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Community, CommunityPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        IEnumerable<CommunityPreviewModel> communityPreviewModels =
            mapper.Map<IEnumerable<Community>, IEnumerable<CommunityPreviewModel>>(communities);

        return communityPreviewModels;
    }

    public IEnumerable<CommunityPreviewModel> GetManaged(int userId)
    {
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
            cfg.CreateMap<User, UserPreviewModel>();
            cfg.CreateMap<Post, PostViewModel>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        IEnumerable<PostViewModel> postViewModels = mapper.Map<IEnumerable<Post>, IEnumerable<PostViewModel>>(_applicationContext.Posts.GetPostsFromCommunity(communityId));
        foreach (var postViewModel in postViewModels)
        {
            postViewModel.Community = mapper.Map<Community, CommunityPreviewModel>(community);
        }
        
        CommunityViewModel communityViewModel = mapper.Map<Community, CommunityViewModel>(community);
        communityViewModel.Posts = postViewModels.ToList();

        return communityViewModel;
    }

    public CommunityPreviewModel Subscribe(int communityId, int userId)
    {
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

        Community updatedCommunity = _applicationContext.Communities.Get(communityId);
        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(updatedCommunity);
        return communityPreviewModel;
    }

    public CommunityPreviewModel Unsubscribe(int communityId, int userId)
    {
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
        
        Community updatedCommunity = _applicationContext.Communities.Get(communityId);
        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(updatedCommunity);
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

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        _applicationContext.Communities.Delete(communityId);
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Community, CommunityPreviewModel>());
        var mapper = new Mapper(mapperConfig);

        CommunityPreviewModel communityPreviewModel = mapper.Map<Community, CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    public PostViewModel AddPost(PostAddModel postAddModel, int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PostAddModel, Post>();

            cfg.CreateMap<Post, PostViewModel>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);

        Post post = mapper.Map<PostAddModel, Post>(postAddModel);
        post.CommunityId = communityId;
        
        _applicationContext.Posts.Add(post);

        PostViewModel postViewModel = mapper.Map<Post, PostViewModel>(post);
        postViewModel.Community = mapper.Map<Community, CommunityPreviewModel>(community);
        return postViewModel;
    }

    public PostViewModel EditPost(PostEditModel postEditModel, int userId)
    {
        Post post = _applicationContext.Posts.Get(postEditModel.Id);
        Community community = _applicationContext.Communities.Get(post.CommunityId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PostEditModel, Post>();
            
            cfg.CreateMap<Post, PostViewModel>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        Post updatedPost = mapper.Map<PostEditModel, Post>(postEditModel);
        _applicationContext.Posts.Update(updatedPost);
        
        PostViewModel postViewModel = mapper.Map<Post, PostViewModel>(updatedPost);
        postViewModel.Community = mapper.Map<Community, CommunityPreviewModel>(community);
        return postViewModel;
    }

    public PostViewModel DeletePost(int postId, int userId)
    {
        Post post = _applicationContext.Posts.Get(postId);
        Community community = _applicationContext.Communities.Get(post.CommunityId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");

        _applicationContext.Posts.Delete(postId);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PostAddModel, Post>();
            
            cfg.CreateMap<Post, PostViewModel>();
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        PostViewModel postViewModel = mapper.Map<Post, PostViewModel>(post);
        postViewModel.Community = mapper.Map<Community, CommunityPreviewModel>(community);
        return postViewModel;
    }
}