using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Users;

public class RegisterUserModel
{
    [Required] public string FirstName { get; set; }
    [Required] public string SecondName { get; set; }
    [EmailAddress] public string Email { get; set; }
    [Required] public string Password { get; set; }
}