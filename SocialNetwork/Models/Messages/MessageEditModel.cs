using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Messages;

public class MessageEditModel
{
    [Required] public long Id { get; set; }
    [Required] public string Text { get; set; }
}