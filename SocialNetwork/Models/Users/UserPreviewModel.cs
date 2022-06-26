namespace SocialNetwork.Models.Users;

public class UserPreviewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string Email { get; set; }
    public bool IsFreeze { get; set; }
}