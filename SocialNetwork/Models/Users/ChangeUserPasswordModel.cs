using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Users;

public class ChangeUserPasswordModel
{
    [EmailAddress] public string Email { get; set; }
    [Required] public string OldPassword { get; set; }
    [Required] public string NewPassword { get; set; }
}