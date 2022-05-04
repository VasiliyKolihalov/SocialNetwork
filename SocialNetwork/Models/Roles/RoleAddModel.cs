using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Roles;

public class RoleAddModel
{
    [Required] public string Name { get; set; }
}