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

    [Route("StartCorrespondence")]
    [HttpPost]
    public ActionResult<CorrespondencePreviewModel> StartCorrespondence(CorrespondeceAddModel correspondenceAddModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.StartCorrespondence(correspondenceAddModel.MessageAddModel, correspondenceAddModel.ParticipantsId, GetUserId());

        return Ok(correspondencePreviewModel);
    }

    [Route("{correspondenceId}/SendMessage")]
    [HttpPut]
    public ActionResult<CorrespondencePreviewModel> SendMessage(MessageAddModel messageAddModel, int correspondenceId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.SendMessage(messageAddModel, correspondenceId, GetUserId());
        
        return Ok(correspondencePreviewModel);
    }

    [Route("{correspondenceId}/EditMessage")]
    [HttpPut]
    public ActionResult<CorrespondencePreviewModel> EditMessage(MessageUpdateModel messageUpdateModel, int correspondenceId)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        CorrespondencePreviewModel correspondencePreviewModel =
            _correspondencesService.EditMessage(messageUpdateModel, GetUserId());

        return Ok(correspondencePreviewModel);
    }

    [Route("{correspondenceId}/DeleteMessage/{messageId}")]
    [HttpPut]
    public ActionResult<CorrespondencePreviewModel> DeleteMessage(int correspondenceId, long messageId)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        CorrespondencePreviewModel correspondencePreviewModel = _correspondencesService.DeleteMessage(messageId, correspondenceId, GetUserId());
        return Ok(correspondencePreviewModel);
    }
}