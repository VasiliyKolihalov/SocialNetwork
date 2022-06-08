using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Roles;

public class RoleAddModel
{
    [Required]  [MaxLength(50)] public string Name { get; set; }
}