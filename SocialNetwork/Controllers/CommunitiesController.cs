using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Extensions;
using SocialNetwork.Models.Comments;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Images;
using SocialNetwork.Models.Posts;
using SocialNetwork.Models.Users;
using SocialNetwork.Services;

namespace SocialNetwork.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CommunitiesController : ControllerBase
{
    private readonly CommunitiesService _communitiesService;

    public CommunitiesController(CommunitiesService communitiesService)
    {
        _communitiesService = communitiesService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommunityPreviewModel>> GetAll()
    {
        IEnumerable<CommunityPreviewModel> communityPreviewModels = _communitiesService.GetAll();
        return Ok(communityPreviewModels);
    }

    [Route("GetFollowed")]
    [HttpGet]
    public ActionResult<IEnumerable<CommunityPreviewModel>> GetFollowed()
    {
        IEnumerable<CommunityPreviewModel> communityPreviewModels = _communitiesService.GetFollowed(this.GetUserIdFromClaims());
        return Ok(communityPreviewModels);
    }

    [Route("GetManaged")]
    [HttpGet]
    public ActionResult<IEnumerable<CommunityPreviewModel>> GetManaged()
    {
        IEnumerable<CommunityPreviewModel> communityPreviewModels = _communitiesService.GetManaged(this.GetUserIdFromClaims());
        return Ok(communityPreviewModels);
    }

    [HttpGet("{communityId}")]
    public ActionResult<CommunityViewModel> GetWithPosts(int communityId)
    {
        CommunityViewModel communityViewModel = _communitiesService.GetWithPosts(communityId, this.GetUserIdFromClaims());
        return Ok(communityViewModel);
    }

    [Route("{communityId}/GetAvatar")]
    [HttpGet]
    public ActionResult<ImageViewModel?> GetCommunityAvatar(int communityId)
    {
        ImageViewModel? imageViewModel = _communitiesService.GetCommunityAvatar(communityId);
        return Ok(imageViewModel);
    }

    [Route("{communityId}/Subscribe")]
    [HttpPost]
    public ActionResult<CommunityPreviewModel> Subscribe(int communityId)
    {
        CommunityPreviewModel communityPreviewModel = _communitiesService.Subscribe(communityId, this.GetUserIdFromClaims());
        return Ok(communityPreviewModel);
    }

    [Route("{communityId}/Unsubscribe")]
    [HttpPost]
    public ActionResult<CommunityPreviewModel> Unsubscribe(int communityId)
    {
        CommunityPreviewModel communityPreviewModel = _communitiesService.Unsubscribe(communityId, this.GetUserIdFromClaims());
        return Ok(communityPreviewModel);
    }

    [HttpPost]
    public ActionResult<CommunityPreviewModel> Post(CommunityAddModel communityAddModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        CommunityPreviewModel communityPreviewModel = _communitiesService.Create(communityAddModel, this.GetUserIdFromClaims());
        return Ok(communityPreviewModel);
    }
    
    [HttpPut]
    public ActionResult<CommunityPreviewModel> Put(CommunityEditModel communityEditModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        CommunityPreviewModel communityPreviewModel = _communitiesService.Edit(communityEditModel, this.GetUserIdFromClaims());
        return Ok(communityPreviewModel);
    }

    [Route("{communityId}/ChangeAvatar")]
    [HttpPut]
    public ActionResult<ImageViewModel> ChangeAvatar(int communityId, ImageAddModel imageAddModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        ImageViewModel imageViewModel =
            _communitiesService.ChangeAvatar(communityId, imageAddModel, this.GetUserIdFromClaims());

        return Ok(imageViewModel);
    }

    [HttpDelete("{communityId}")]
    public ActionResult<CommunityPreviewModel> Delete(int communityId)
    {
        CommunityPreviewModel communityPreviewModel = _communitiesService.Delete(communityId, this.GetUserIdFromClaims());
        return Ok(communityPreviewModel);
    }

    [Route("{communityId}/Posts/Add")]
    [HttpPost]
    public ActionResult<PostViewModel> AddPost(PostAddModel postAddModel, int communityId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        PostViewModel postViewModel = _communitiesService.AddPost(postAddModel, communityId, this.GetUserIdFromClaims());
        return Ok(postViewModel);
    }
    
    [Route("Posts/Edit")]
    [HttpPut]
    public ActionResult<PostViewModel> EditPost(PostEditModel postEditModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        PostViewModel postViewModel = _communitiesService.EditPost(postEditModel, this.GetUserIdFromClaims());
        return Ok(postViewModel);
    }
    
    
    [Route("Posts/{postId}/Delete")]
    [HttpDelete]
    public ActionResult<PostViewModel> DeletePost(int communityId, int postId)
    {
        PostViewModel postViewModel = _communitiesService.DeletePost(postId, this.GetUserIdFromClaims());
        return Ok(postViewModel);
    }
    
    [Route("Posts/{postId}/GetUsersWhoLike")]
    [HttpGet]
    public ActionResult<IEnumerable<UserPreviewModel>> GetUsersWhoLikePost(int postId)
    {
        IEnumerable<UserPreviewModel> userPreviewModels = _communitiesService.GetUsersWhoLikePost(postId);
        return Ok(userPreviewModels);
    }

    [Route("Posts/{postId}/Like")]
    [HttpPost]
    public ActionResult<PostViewModel> LikePost(int postId)
    {
        PostViewModel postViewModel = _communitiesService.LikePost(postId, this.GetUserIdFromClaims());
        return Ok(postViewModel);
    }

    [Route("Posts/{postId}/DeleteLike")]
    [HttpDelete]
    public ActionResult<PostViewModel> DeleteLikeFromPost(int postId)
    {
        PostViewModel postViewModel = _communitiesService.DeleteLikeFromPost(postId, this.GetUserIdFromClaims());
        return Ok(postViewModel);
    }

    [Route("Posts/{postId}/Comments")]
    [HttpGet]
    public ActionResult<IEnumerable<CommentViewModel>> GetPostComments(int postId)
    {
        IEnumerable<CommentViewModel> commentViewModels = _communitiesService.GetPostComments(postId, this.GetUserIdFromClaims());
        return Ok(commentViewModels);
    }

    [Route("Posts/{postId}/Comments/Add")]
    [HttpPost]
    public ActionResult<CommentViewModel> AddComment(CommentAddModel commentAddModel, int postId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        CommentViewModel commentViewModel =
            _communitiesService.AddComment(commentAddModel, postId, this.GetUserIdFromClaims());

        return Ok(commentViewModel);
    }

    [Route("Posts/Comments/Edit")]
    [HttpPut]
    public ActionResult<CommentViewModel> EditComment(CommentEditModel commentEditModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        CommentViewModel commentViewModel =
            _communitiesService.EditComment(commentEditModel, this.GetUserIdFromClaims());

        return Ok(commentViewModel);
    }
    
    [Route("Posts/Comments/{commentId}/Delete")]
    [HttpDelete]
    public ActionResult<CommentViewModel> DeleteComment(int commentId)
    {
        CommentViewModel commentViewModels = _communitiesService.DeleteComment(commentId, this.GetUserIdFromClaims());
        return Ok(commentViewModels);
    }
    
    [Route("Posts/Comments/{commentId}/GetUsersWhoLike")]
    [HttpGet]
    public ActionResult<IEnumerable<UserPreviewModel>> GetUsersWhoLikeComment(int commentId)
    {
        IEnumerable<UserPreviewModel> userPreviewModels = _communitiesService.GetUsersWhoLikeComment(commentId);
        return Ok(userPreviewModels);
    }

    [Route("Posts/Comments/{commentId}/Like")]
    [HttpPost]
    public ActionResult<CommentViewModel> LikeComment(int commentId)
    {
        CommentViewModel commentViewModel = _communitiesService.LikeComment(commentId, this.GetUserIdFromClaims());
        return Ok(commentViewModel);
    }
    
    [Route("Posts/Comments/{commentId}/DeleteLike")]
    [HttpDelete]
    public ActionResult<CommentViewModel> DeleteLikeFromComment(int commentId)
    {
        CommentViewModel commentViewModel = _communitiesService.DeleteLikeFromComment(commentId, this.GetUserIdFromClaims());
        return Ok(commentViewModel);
    }
}