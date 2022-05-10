using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Posts;

public class PostAddModel
{
    [Required] public string Content { get; set; }
}