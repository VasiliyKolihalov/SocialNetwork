using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Messages;

public class MessageAddModel
{
    [Required] public string Text { get; set; }
}