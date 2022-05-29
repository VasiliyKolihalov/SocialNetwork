using SocialNetwork.Models.Communities;

namespace SocialNetwork.Models.Users;

public class UserViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string Email { get; set; }
    
    public List<CommunityPreviewModel> Communities { get; set; }
    public List<UserPreviewModel> Friends { get; set; }
}