using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Comments;

public class CommentAddModel
{
    [Required] [MaxLength(500)] public string Content { get; set; }
}