namespace SocialNetwork.Models.Users;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsConfirmEmail { get; set; }
    public bool IsFreeze { get; set; }
}