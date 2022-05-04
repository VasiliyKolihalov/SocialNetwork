using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Roles;

public class RoleUpdateModel
{
    [Required] public int Id { get; set; }
    [Required] public string Name { get; set; }
}