using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Roles;

public class RoleEditModel
{
    [Required] public int Id { get; set; }
    [Required] [MaxLength(50)] public string Name { get; set; }
}