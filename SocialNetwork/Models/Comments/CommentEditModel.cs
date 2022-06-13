using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Comments;

public class CommentEditModel
{
    [Required] public int Id { get; set; }
    [Required] [MaxLength(500)] public string Content { get; set; }
}