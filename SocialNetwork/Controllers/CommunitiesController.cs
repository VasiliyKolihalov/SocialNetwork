﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Posts;
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

    private int GetUserId()
    {
        int userId = Convert.ToInt32(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
        return userId;
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
        IEnumerable<CommunityPreviewModel> communityPreviewModels = _communitiesService.GetFollowed(GetUserId());
        return Ok(communityPreviewModels);
    }

    [Route("GetManaged")]
    [HttpGet]
    public ActionResult<IEnumerable<CommunityPreviewModel>> GetManaged()
    {
        IEnumerable<CommunityPreviewModel> communityPreviewModels = _communitiesService.GetManaged(GetUserId());
        return Ok(communityPreviewModels);
    }

    [Route("{id}")]
    [HttpGet]
    public ActionResult<CommunityViewModel> GetWithPosts(int id)
    {
        CommunityViewModel communityViewModel = _communitiesService.GetWithPosts(id);
        return Ok(communityViewModel);
    }

    [Route("{communityId}/Subscribe")]
    [HttpPut]
    public ActionResult<CommunityPreviewModel> Subscribe(int communityId)
    {
        CommunityPreviewModel communityPreviewModel = _communitiesService.Subscribe(communityId, GetUserId());
        return Ok(communityPreviewModel);
    }

    [Route("{communityId}/Unsubscribe")]
    [HttpPut]
    public ActionResult<CommunityPreviewModel> Unsubscribe(int communityId)
    {
        CommunityPreviewModel communityPreviewModel = _communitiesService.Unsubscribe(communityId, GetUserId());
        return Ok(communityPreviewModel);
    }

    [HttpPost]
    public ActionResult<CommunityPreviewModel> Post(CommunityAddModel communityAddModel)
    {
        CommunityPreviewModel communityPreviewModel = _communitiesService.Create(communityAddModel, GetUserId());
        return Ok(communityPreviewModel);
    }
    
    [HttpPut]
    public ActionResult<CommunityPreviewModel> Put(CommunityEditModel communityEditModel)
    {
        CommunityPreviewModel communityPreviewModel = _communitiesService.Edit(communityEditModel, GetUserId());
        return Ok(communityPreviewModel);
    }
    
    [HttpDelete("{communityId}")]
    public ActionResult<CommunityPreviewModel> Delete(int communityId)
    {
        CommunityPreviewModel communityPreviewModel = _communitiesService.Delete(communityId, GetUserId());
        return Ok(communityPreviewModel);
    }

    [Route("{communityId}/AddPost")]
    [HttpPut]
    public ActionResult<PostViewModel> AddPost(PostAddModel postAddModel, int communityId)
    {
        PostViewModel postViewModel = _communitiesService.AddPost(postAddModel, communityId, GetUserId());
        return Ok(postViewModel);
    }
    
    [Route("EditPost")]
    [HttpPut]
    public ActionResult<PostViewModel> EditPost(PostEditModel postEditModel)
    {
        PostViewModel postViewModel = _communitiesService.EditPost(postEditModel, GetUserId());
        return Ok(postViewModel);
    }
    
    [Route("DeletePost/{postId}")]
    [HttpPut]
    public ActionResult<PostViewModel> DeletePost(int communityId, int postId)
    {
        PostViewModel postViewModel = _communitiesService.DeletePost(postId, GetUserId());
        return Ok(postViewModel);
    }
}