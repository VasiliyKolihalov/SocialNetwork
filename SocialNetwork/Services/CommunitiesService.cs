using AutoMapper;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Comments;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Images;
using SocialNetwork.Models.Posts;
using SocialNetwork.Models.Users;
using SocialNetwork.Repositories;

namespace SocialNetwork.Services;

public class CommunitiesService
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMapper _mapper;

    public CommunitiesService(IApplicationContext applicationContext, IMapper mapper)
    {
        _applicationContext = applicationContext;
        _mapper = mapper;
    }

    public IEnumerable<CommunityPreviewModel> GetAll()
    {
        IEnumerable<Community> communities = _applicationContext.Communities.GetAll();

        IEnumerable<CommunityPreviewModel> communityPreviewModels = _mapper.Map<IEnumerable<CommunityPreviewModel>>(communities);

        return communityPreviewModels;
    }

    public IEnumerable<CommunityPreviewModel> GetFollowed(int userId)
    {
        IEnumerable<Community> communities = _applicationContext.Communities.GetFollowedCommunity(userId);

        IEnumerable<CommunityPreviewModel> communityPreviewModels = _mapper.Map<IEnumerable<CommunityPreviewModel>>(communities);

        return communityPreviewModels;
    }
 
    public IEnumerable<CommunityPreviewModel> GetManaged(int userId)
    {
        IEnumerable<Community> communities = _applicationContext.Communities.GetManagedCommunity(userId);

        IEnumerable<CommunityPreviewModel> communityPreviewModels = _mapper.Map<IEnumerable<CommunityPreviewModel>>(communities);

        return communityPreviewModels;
    }

    public CommunityViewModel GetWithPosts(int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);
        IEnumerable<Post> posts = _applicationContext.Posts.GetPostsFromCommunity(communityId); 

        IEnumerable<PostViewModel> postViewModels = _mapper.Map<IEnumerable<PostViewModel>>(posts);
        foreach (var postViewModel in postViewModels)
        {
            postViewModel.Community = _mapper.Map<CommunityPreviewModel>(community);

            IEnumerable<Image> images = _applicationContext.Images.GetPostImages(postViewModel.Id);
            postViewModel.Images = _mapper.Map<List<ImageViewModel>>(images);
            
            postViewModel.IsUserLike = _applicationContext.Posts.IsUserLike(userId, postViewModel.Id);
        }
        
        CommunityViewModel communityViewModel = _mapper.Map<CommunityViewModel>(community);
        communityViewModel.Posts = postViewModels.ToList();

        Image? avatar = _applicationContext.Images.GetCommunityAvatar(communityViewModel.Id);
        communityViewModel.Avatar = avatar == null ? null : _mapper.Map<ImageViewModel>(avatar);

        return communityViewModel;
    }

    public ImageViewModel? GetCommunityAvatar(int communityId)
    {
        _applicationContext.Communities.Get(communityId);

        Image? image = _applicationContext.Images.GetCommunityAvatar(communityId);

        if (image == null)
            return null;

        ImageViewModel imageViewModel = _mapper.Map<ImageViewModel>(image);
        return imageViewModel;
    }

    public CommunityPreviewModel Subscribe(int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);

        if (community.Users.Any(x => x.Id == userId))
            throw new BadRequestException("User is already in community");
        
        _applicationContext.Communities.SubscribeUserToCommunity(communityId, userId);
        
        Community updatedCommunity = _applicationContext.Communities.Get(communityId);
        CommunityPreviewModel communityPreviewModel = _mapper.Map<CommunityPreviewModel>(updatedCommunity);
        return communityPreviewModel;
    }

    public CommunityPreviewModel Unsubscribe(int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);
        
        if (!community.Users.Any(x => x.Id == userId))
            throw new BadRequestException("User is not member of community");
        
        _applicationContext.Communities.UnsubscribeUserFromCommunity(communityId, userId);

        Community updatedCommunity = _applicationContext.Communities.Get(communityId);
        CommunityPreviewModel communityPreviewModel = _mapper.Map<CommunityPreviewModel>(updatedCommunity);
        return communityPreviewModel;
    }

    public CommunityPreviewModel Create(CommunityAddModel communityAddModel, int userId)
    {
        User author = _applicationContext.Users.Get(userId);

        Community community = _mapper.Map<Community>(communityAddModel);
        community.Author = author;
        
        _applicationContext.Communities.Add(community);
        _applicationContext.Images.SetDefaultValueForCommunityAvatar(community.Id);

        CommunityPreviewModel communityPreviewModel = _mapper.Map<CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    public CommunityPreviewModel Edit(CommunityEditModel communityEditModel, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityEditModel.Id);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        Community updatedCommunity = _mapper.Map<Community>(communityEditModel);
        _applicationContext.Communities.Update(updatedCommunity);

        CommunityPreviewModel communityPreviewModel = _mapper.Map<CommunityPreviewModel>(updatedCommunity);
        return communityPreviewModel;
    }

    public ImageViewModel ChangeAvatar(int communityId, ImageAddModel imageAddModel, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");

        Image newAvatar;
        try
        {
            newAvatar = _mapper.Map<Image>(imageAddModel);
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

        ImageViewModel imageViewModel = _mapper.Map<ImageViewModel>(newAvatar);
        return imageViewModel;
    }

    public CommunityPreviewModel Delete(int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId); 

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        _applicationContext.Communities.Delete(communityId);

        CommunityPreviewModel communityPreviewModel = _mapper.Map<CommunityPreviewModel>(community);
        return communityPreviewModel;
    }

    #region Posts
    
    public PostViewModel AddPost(PostAddModel postAddModel, int communityId, int userId)
    {
        Community community = _applicationContext.Communities.Get(communityId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");

        IEnumerable<Image> images;
        try
        {
            images = _mapper.Map<IEnumerable<Image>>(postAddModel.Images);
        }
        catch (Exception e)
        {
            throw new BadRequestException(e.Message);
        }
        
        Post post = _mapper.Map<Post>(postAddModel);
        post.CommunityId = communityId;
        
        _applicationContext.Posts.Add(post);
        foreach (var image in images)
        {
            image.UserId = userId;
           _applicationContext.Images.Add(image); 
           _applicationContext.Images.AddImageToPost(post.Id, image.Id);
        }

        PostViewModel postViewModel = _mapper.Map<PostViewModel>(post);
        postViewModel.Community = _mapper.Map<CommunityPreviewModel>(community);
        postViewModel.Images = _mapper.Map<List<ImageViewModel>>(images);
        return postViewModel;
    }

    public PostViewModel EditPost(PostEditModel postEditModel, int userId)
    {
        Post post = _applicationContext.Posts.Get(postEditModel.Id);
        Community community = _applicationContext.Communities.Get(post.CommunityId);

        if (community.Author.Id != userId)
            throw new NotFoundException("Community not found");
        
        Post updatedPost = _mapper.Map<Post>(postEditModel);
        _applicationContext.Posts.Update(updatedPost);
        
        PostViewModel postViewModel = _mapper.Map<PostViewModel>(updatedPost);
        postViewModel.Community = _mapper.Map<CommunityPreviewModel>(community);
        IEnumerable<Image> images = _applicationContext.Images.GetPostImages(postEditModel.Id);
        postViewModel.Images = _mapper.Map<List<ImageViewModel>>(images);
        postViewModel.IsUserLike = _applicationContext.Posts.IsUserLike(userId, postViewModel.Id);

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

        PostViewModel postViewModel = _mapper.Map<PostViewModel>(post);
        postViewModel.Community = _mapper.Map<CommunityPreviewModel>(community);
        postViewModel.Images = _mapper.Map<List<ImageViewModel>>(images);
        postViewModel.IsUserLike = _applicationContext.Posts.IsUserLike(userId, postViewModel.Id);

        return postViewModel;
    }
    
    #endregion
    
    #region Posts likes

    public IEnumerable<UserPreviewModel> GetUsersWhoLikePost(int postId)
    {
        _applicationContext.Posts.Get(postId);
        
        IEnumerable<User> users = _applicationContext.Users.GetUsersWhoLikePost(postId);

        IEnumerable<UserPreviewModel> userPreviewModels = _mapper.Map<IEnumerable<UserPreviewModel>>(users);

        return userPreviewModels;
    }

    public PostViewModel LikePost(int postId, int userId)
    {
        Post post = _applicationContext.Posts.Get(postId);

        if (_applicationContext.Posts.IsUserLike(userId, postId))
            throw new BadRequestException("User already like post");
        
        _applicationContext.Posts.AddLike(userId, postId);

        PostViewModel postViewModel = _mapper.Map<PostViewModel>(post);
        Community community = _applicationContext.Communities.Get(post.CommunityId);
        postViewModel.Community = _mapper.Map<CommunityPreviewModel>(community);
        IEnumerable<Image> images = _applicationContext.Images.GetPostImages(postId);
        postViewModel.Images = _mapper.Map<List<ImageViewModel>>(images);
        postViewModel.IsUserLike = true;
        postViewModel.LikesCount++;

        return postViewModel;
    }

    public PostViewModel DeleteLikeFromPost(int postId, int userId)
    {
        Post post = _applicationContext.Posts.Get(postId);

        if (!_applicationContext.Posts.IsUserLike(userId, postId))
            throw new BadRequestException("User didn't like post");
        
        _applicationContext.Posts.DeleteLike(userId, postId);

        PostViewModel postViewModel = _mapper.Map<PostViewModel>(post);
        Community community = _applicationContext.Communities.Get(post.CommunityId);
        postViewModel.Community = _mapper.Map<CommunityPreviewModel>(community);
        IEnumerable<Image> images = _applicationContext.Images.GetPostImages(postId);
        postViewModel.Images = _mapper.Map<List<ImageViewModel>>(images);
        postViewModel.IsUserLike = false;
        postViewModel.LikesCount--;

        return postViewModel;
    }
    
    #endregion

    #region  Comments
    
    public IEnumerable<CommentViewModel> GetPostComments(int postId, int userId)
    {
        _applicationContext.Posts.Get(postId);

        IEnumerable<Comment> comments = _applicationContext.Comments.GetPostComments(postId);

        IEnumerable<CommentViewModel> commentViewModels = _mapper.Map<IEnumerable<CommentViewModel>>(comments);
        foreach (var commentViewModel in commentViewModels)
        {
            commentViewModel.IsUserLike = _applicationContext.Comments.IsUserLike(userId,commentViewModel.Id);
        }

        return commentViewModels;
    }

    public CommentViewModel AddComment(CommentAddModel commentAddModel, int postId, int userId)
    {
        _applicationContext.Posts.Get(postId);
        
        Comment comment = _mapper.Map<Comment>(commentAddModel);
        User user = _applicationContext.Users.Get(userId);
        comment.User = user;
        comment.PostId = postId;
        
        _applicationContext.Comments.Add(comment);

        CommentViewModel commentViewModel = _mapper.Map<CommentViewModel>(comment);
        return commentViewModel;
    }

    public CommentViewModel EditComment(CommentEditModel commentEditModel, int userId)
    {
        Comment comment = _applicationContext.Comments.Get(commentEditModel.Id);

        if (comment.User.Id != userId)
            throw new NotFoundException("Comment not found");

        Comment updatedComment = _mapper.Map<Comment>(commentEditModel);
        _applicationContext.Comments.Update(updatedComment);
        
        CommentViewModel commentViewModel = _mapper.Map<CommentViewModel>(updatedComment);
        commentViewModel.User = _mapper.Map<UserPreviewModel>(comment.User);
        commentViewModel.IsUserLike = _applicationContext.Comments.IsUserLike(userId,comment.Id);
        return commentViewModel;
    }

    public CommentViewModel DeleteComment(int commentId, int userId)
    {
        Comment comment = _applicationContext.Comments.Get(commentId);

        if (comment.User.Id != userId)
            throw new NotFoundException("Comment not found");
        
        _applicationContext.Comments.Delete(commentId);
        
        CommentViewModel commentViewModel = _mapper.Map<CommentViewModel>(comment);
        commentViewModel.IsUserLike = _applicationContext.Comments.IsUserLike(userId,commentId);
        return commentViewModel;
    }
    
    #endregion
    
    #region Comments likes

     public IEnumerable<UserPreviewModel> GetUsersWhoLikeComment(int commentId)
    {
        _applicationContext.Comments.Get(commentId);
        
        IEnumerable<User> users = _applicationContext.Users.GetUsersWhoLikeComment(commentId);

        IEnumerable<UserPreviewModel> userPreviewModels = _mapper.Map<IEnumerable<UserPreviewModel>>(users);

        return userPreviewModels;
    }

    public CommentViewModel LikeComment(int commentId, int userId)
    {
        Comment comment = _applicationContext.Comments.Get(commentId);

        if (_applicationContext.Comments.IsUserLike(userId, commentId))
            throw new BadRequestException("User already like comment");
        
        _applicationContext.Comments.AddLike(userId, commentId);

        CommentViewModel commentViewModel = _mapper.Map<CommentViewModel>(comment);
        commentViewModel.IsUserLike = true;
        commentViewModel.LikesCount++;

        return commentViewModel;
    }

    public CommentViewModel DeleteLikeFromComment(int commentId, int userId)
    {
        Comment comment = _applicationContext.Comments.Get(commentId);

        if (!_applicationContext.Comments.IsUserLike(userId, commentId))
            throw new BadRequestException("User didn't like comment");
        
        _applicationContext.Comments.DeleteLike(userId, commentId);
        
        CommentViewModel commentViewModel = _mapper.Map<CommentViewModel>(comment);
        commentViewModel.IsUserLike = false;
        commentViewModel.LikesCount--;

        return commentViewModel;
    }

    #endregion
    
}