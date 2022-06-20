using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Extensions;
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

    [HttpGet]
    public ActionResult<IEnumerable<CorrespondencePreviewModel>> GetAll()
    {
        IEnumerable<CorrespondencePreviewModel> correspondencePreviewModel = _correspondencesService.GetAll(this.GetUserIdFromClaims());
        return Ok(correspondencePreviewModel);
    }

    [HttpGet("{id}")]
    public ActionResult<CorrespondenceViewModel> GetWithMessages(int id)
    {
        CorrespondenceViewModel correspondenceViewModel = _correspondencesService.GetWithMessages(id, this.GetUserIdFromClaims());
        return Ok(correspondenceViewModel);
    }

    [HttpPost]
    public ActionResult<CorrespondencePreviewModel> StartCorrespondence(CorrespondenceAddModel correspondenceAddModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.StartCorrespondence(correspondenceAddModel, this.GetUserIdFromClaims());

        return Ok(correspondencePreviewModel);
    }
    
    [HttpPut]
    public ActionResult<CorrespondencePreviewModel> Put(CorrespondenceEditModel correspondenceEditModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.Edit(correspondenceEditModel, this.GetUserIdFromClaims());

        return Ok(correspondencePreviewModel);
    }

    [HttpDelete("{correspondenceId}")]
    public ActionResult<CorrespondencePreviewModel> Delete(int correspondenceId)
    {
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.Delete(correspondenceId, this.GetUserIdFromClaims());
        
        return Ok(correspondencePreviewModel);
    }

    [Route("{correspondenceId}/AddUser/{userId}")]
    [HttpPost]
    public ActionResult<CorrespondencePreviewModel> AddUserToCorrespondence(int correspondenceId, int userId)
    {
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.AddUserToCorrespondence(correspondenceId, userId, this.GetUserIdFromClaims());

        return Ok(correspondencePreviewModel);
    }
    
    [Route("{correspondenceId}/DeleteUser/{userId}")]
    [HttpDelete]
    public ActionResult<CorrespondencePreviewModel> DeleteUserFromCorrespondence(int correspondenceId, int userId)
    {
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.DeleteUserFromCorrespondence(correspondenceId, userId, this.GetUserIdFromClaims());

        return Ok(correspondencePreviewModel);
    }
    
    [Route("{correspondenceId}/Messages/Send")]
    [HttpPost]
    public ActionResult<MessageViewModel> SendMessage(MessageAddModel messageAddModel, int correspondenceId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        MessageViewModel messageViewModel = _correspondencesService.SendMessage(messageAddModel, correspondenceId, this.GetUserIdFromClaims());
        
        return Ok(messageViewModel);
    }

    [Route("Messages/Edit")]
    [HttpPut]
    public ActionResult<MessageViewModel> EditMessage(MessageEditModel messageEditModel)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        MessageViewModel messageViewModel = _correspondencesService.EditMessage(messageEditModel, this.GetUserIdFromClaims());

        return Ok(messageViewModel);
    }

    [Route("Messages/{messageId}/Delete")]
    [HttpDelete]
    public ActionResult<MessageViewModel> DeleteMessage(long messageId)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        MessageViewModel messageViewModel = _correspondencesService.DeleteMessage(messageId, this.GetUserIdFromClaims());
        return Ok(messageViewModel);
    }
}