using System.ComponentModel.DataAnnotations;
using SocialNetwork.Constants;

namespace SocialNetwork.Models.Messages;

public class MessageAddModel
{
    [Required] [MaxLength(500)] public string Text { get; set; }
}