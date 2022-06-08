using System.Security.Cryptography;
using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Images;
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
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        });
        var mapper = new Mapper(mapperConfig);

        IEnumerable<PostViewModel> postViewModels = mapper.Map<IEnumerable<Post>, IEnumerable<PostViewModel>>(_applicationContext.Posts.GetPostsFromCommunity(communityId));
        foreach (var postViewModel in postViewModels)
        {
            postViewModel.Community = mapper.Map<Community, CommunityPreviewModel>(community);

            IEnumerable<Image> images = _applicationContext.Images.GetPostImages(postViewModel.Id);
            postViewModel.Images = mapper.Map<IEnumerable<Image>, List<ImageViewModel>>(images);
        }
        
        CommunityViewModel communityViewModel = mapper.Map<Community, CommunityViewModel>(community);
        communityViewModel.Posts = postViewModels.ToList();

        Image? avatar = _applicationContext.Images.GetCommunityAvatar(communityViewModel.Id);
        communityViewModel.Avatar = avatar == null ? null : mapper.Map<Image, ImageViewModel>(avatar);

        return communityViewModel;
    }

    public ImageViewModel? GetCommunityAvatar(int communityId)
    {
        _applicationContext.Communities.Get(communityId);

        Image? image = _applicationContext.Images.GetCommunityAvatar(communityId);

        if (image == null)
            return null;
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        });
        var mapper = new Mapper(mapperConfig);

        ImageViewModel imageViewModel = mapper.Map<Image, ImageViewModel>(image);
        return imageViewModel;
    }

    public CommunityPreviewModel Subscribe(int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);

        if (community.Users.Any(x => x.Id == userId))
            throw new BadRequestException("User is already in community");
        
        _applicationContext.Communities.SubscribeUserToCommunity(communityId, userId);
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Community, CommunityPreviewModel>());
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
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Community, CommunityPreviewModel>());
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
        _applicationContext.Images.SetDefaultValueForCommunityAvatar(community.Id);

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

    public ImageViewModel ChangeAvatar(int communityId, ImageAddModel imageAddModel, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ImageAddModel, Image>().ForMember(nameof(Image.ImageData), opt =>
                opt.MapFrom(x => Convert.FromBase64String(x.ImageData)));

            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
        });
        var mapper = new Mapper(mapperConfig);

        Image newAvatar;
        try
        {
            newAvatar = mapper.Map<ImageAddModel, Image>(imageAddModel);
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }

        Image? oldAvatar = _applicationContext.Images.GetCommunityAvatar(communityId);
        if(oldAvatar != null)
            _applicationContext.Images.Delete(oldAvatar.Id);

        newAvatar.UserId = userId;
        _applicationContext.Images.Add(newAvatar);
        _applicationContext.Images.UpdateCommunityAvatar(communityId, newAvatar.Id);

        ImageViewModel imageViewModel = mapper.Map<Image, ImageViewModel>(newAvatar);
        return imageViewModel;
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
            cfg.CreateMap<ImageAddModel, Image>().ForMember(nameof(Image.ImageData), opt =>
                opt.MapFrom(x => Convert.FromBase64String(x.ImageData)));

            cfg.CreateMap<Post, PostViewModel>();
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        IEnumerable<Image> images;
        try
        {
            images = mapper.Map<List<ImageAddModel>, IEnumerable<Image>>(postAddModel.Images);
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
        
        Post post = mapper.Map<PostAddModel, Post>(postAddModel);
        post.CommunityId = communityId;
        
        _applicationContext.Posts.Add(post);
        foreach (var image in images)
        {
            image.UserId = userId;
           _applicationContext.Images.Add(image); 
           _applicationContext.Images.AddImageToPost(post.Id, image.Id);
        }

        PostViewModel postViewModel = mapper.Map<Post, PostViewModel>(post);
        postViewModel.Community = mapper.Map<Community, CommunityPreviewModel>(community);
        postViewModel.Images = mapper.Map<IEnumerable<Image>, List<ImageViewModel>>(images);
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
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        Post updatedPost = mapper.Map<PostEditModel, Post>(postEditModel);
        _applicationContext.Posts.Update(updatedPost);
        
        PostViewModel postViewModel = mapper.Map<Post, PostViewModel>(updatedPost);
        postViewModel.Community = mapper.Map<Community, CommunityPreviewModel>(community);
        IEnumerable<Image> images = _applicationContext.Images.GetPostImages(postEditModel.Id);
        postViewModel.Images = mapper.Map<IEnumerable<Image>, List<ImageViewModel>>(images);
        
        return postViewModel;
    }

    public PostViewModel DeletePost(int postId, int userId)
    {
        Post post = _applicationContext.Posts.Get(postId);
        Community community = _applicationContext.Communities.Get(post.CommunityId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        IEnumerable<Image> images = _applicationContext.Images.GetPostImages(postId);

        _applicationContext.Posts.Delete(postId);

        foreach (var image in images)
        {
            _applicationContext.Images.Delete(image.Id);    
        }

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Post, PostViewModel>();
            cfg.CreateMap<Image, ImageViewModel>().ForMember(nameof(ImageViewModel.ImageData), opt =>
                opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
            cfg.CreateMap<Community, CommunityPreviewModel>();
        });
        var mapper = new Mapper(mapperConfig);
        
        PostViewModel postViewModel = mapper.Map<Post, PostViewModel>(post);
        postViewModel.Community = mapper.Map<Community, CommunityPreviewModel>(community);
        postViewModel.Images = mapper.Map<IEnumerable<Image>, List<ImageViewModel>>(images);
        
        return postViewModel;
    }
}