using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Communities;

public class CommunityAddModel
{
    [Required] public string Name { get; set; }
    [Required] public string Description { get; set; }
}