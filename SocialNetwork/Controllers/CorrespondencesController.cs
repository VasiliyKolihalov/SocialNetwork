using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Models.Correspondences;
using SocialNetwork.Models.Messages;
using SocialNetwork.Services;

namespace SocialNetwork.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CorrespondencesController : ControllerBase
{
    private readonly CorrespondencesService _correspondencesService;

    public CorrespondencesController(CorrespondencesService correspondencesService)
    {
        _correspondencesService = correspondencesService;
    }

    private int GetUserId()
    {
        int userId = Convert.ToInt32(User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
        return userId;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CorrespondencePreviewModel>> GetAll()
    {
        IEnumerable<CorrespondencePreviewModel> correspondencePreviewModel = _correspondencesService.GetAll(GetUserId());
        return Ok(correspondencePreviewModel);
    }

    [HttpGet("{id}")]
    public ActionResult<CorrespondenceViewModel> GetWithMessages(int id)
    {
        CorrespondenceViewModel correspondenceViewModel = _correspondencesService.GetWithMessages(id, GetUserId());
        return Ok(correspondenceViewModel);
    }

    [HttpPost]
    public ActionResult<CorrespondencePreviewModel> StartCorrespondence(CorrespondenceAddModel correspondenceAddModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.StartCorrespondence(correspondenceAddModel, GetUserId());

        return Ok(correspondencePreviewModel);
    }
    
    [HttpPut]
    public ActionResult<CorrespondencePreviewModel> Put(CorrespondenceEditModel correspondenceEditModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.Edit(correspondenceEditModel, GetUserId());

        return Ok(correspondencePreviewModel);
    }

    [HttpDelete("{correspondenceId}")]
    public ActionResult<CorrespondencePreviewModel> Delete(int correspondenceId)
    {
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.Delete(correspondenceId, GetUserId());
        
        return Ok(correspondencePreviewModel);
    }

    [Route("{correspondenceId}/addUser/{userId}")]
    [HttpPut]
    public ActionResult<CorrespondencePreviewModel> AddUserToCorrespondence(int correspondenceId, int userId)
    {
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.AddUserToCorrespondence(correspondenceId, userId, GetUserId());

        return Ok(correspondencePreviewModel);
    }
    
    [Route("{correspondenceId}/deleteUser/{userId}")]
    [HttpPut]
    public ActionResult<CorrespondencePreviewModel> DeleteUserFromCorrespondence(int correspondenceId, int userId)
    {
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.DeleteUserFromCorrespondence(correspondenceId, userId, GetUserId());

        return Ok(correspondencePreviewModel);
    }
    
    [Route("{correspondenceId}/SendMessage")]
    [HttpPut]
    public ActionResult<MessageViewModel> SendMessage(MessageAddModel messageAddModel, int correspondenceId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        MessageViewModel messageViewModel = _correspondencesService.SendMessage(messageAddModel, correspondenceId, GetUserId());
        
        return Ok(messageViewModel);
    }

    [Route("EditMessage")]
    [HttpPut]
    public ActionResult<MessageViewModel> EditMessage(MessageEditModel messageEditModel)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        MessageViewModel messageViewModel = _correspondencesService.EditMessage(messageEditModel, GetUserId());

        return Ok(messageViewModel);
    }

    [Route("DeleteMessage/{messageId}")]
    [HttpPut]
    public ActionResult<MessageViewModel> DeleteMessage(long messageId)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        MessageViewModel messageViewModel = _correspondencesService.DeleteMessage(messageId, GetUserId());
        return Ok(messageViewModel);
    }
}