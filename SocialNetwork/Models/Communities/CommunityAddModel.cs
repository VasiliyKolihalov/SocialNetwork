using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Communities;

public class CommunityAddModel
{
    [Required] [MaxLength(50)] public string Name { get; set; }
    [Required] [MaxLength(100)] public string Description { get; set; }
}