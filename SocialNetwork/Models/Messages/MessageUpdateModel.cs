using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Messages;

public class MessageUpdateModel
{
    [Required] public long Id { get; set; }
    [Required] public string Text { get; set; }
}