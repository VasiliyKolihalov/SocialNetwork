using System.ComponentModel.DataAnnotations;
using SocialNetwork.Constants;

namespace SocialNetwork.Models.Messages;

public class MessageEditModel
{
    [Required] public long Id { get; set; }
    
    [Required] [MaxLength(500)] public string Text { get; set; }
}