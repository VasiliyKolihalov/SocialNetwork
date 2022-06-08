using System.ComponentModel.DataAnnotations;
using SocialNetwork.Constants;
using SocialNetwork.Models.Images;

namespace SocialNetwork.Models.Posts;

public class PostAddModel
{
    [Required] [MaxLength(1000)] public string Content { get; set; }
    [MaxLength(10)] public List<ImageAddModel> Images { get; set; }
}