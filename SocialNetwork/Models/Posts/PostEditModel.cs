using System.ComponentModel.DataAnnotations;
using SocialNetwork.Constants;
using SocialNetwork.Models.Images;

namespace SocialNetwork.Models.Posts;

public class PostEditModel
{
    [Required] public int Id { get; set; }
    [Required] [MaxLength(1000)] public string Content { get; set; }
}