using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Posts;

public class PostEditModel
{
    [Required] public int Id { get; set; }
    [Required] public string Content { get; set; }
}