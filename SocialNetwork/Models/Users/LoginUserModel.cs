using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Users;

public class LoginUserModel
{
    [EmailAddress] public string Email { get; set; }
    [Required] public string Password { get; set; }
}