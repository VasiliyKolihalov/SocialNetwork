using System.ComponentModel.DataAnnotations;

namespace SocialNetwork.Models.Users;

public class UserPutModel
{
    [Required] public int Id { get; set; }
    [Required] public string FirstName { get; set; }
    [Required] public string SecondName { get; set; }
    [EmailAddress] public string Email { get; set; }
}