using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Users;

public class RegisterUserModel
{
    [Required] [MaxLength(50)] public string FirstName { get; set; }
    [Required] [MaxLength(50)] public string SecondName { get; set; }
    [EmailAddress] [MaxLength(100)]public string Email { get; set; }
    [Required] [MinLength(8)] public string Password { get; set; }
}