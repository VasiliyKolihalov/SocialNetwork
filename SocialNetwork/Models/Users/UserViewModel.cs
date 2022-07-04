using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Images;

namespace SocialNetwork.Models.Users;

public class UserViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string Email { get; set; }
    public bool IsConfirmEmail { get; set; }
    public bool IsFreeze { get; set; }

    public ImageViewModel? Avatar { get; set; }
    public List<ImageViewModel> Photos { get; set; }
    public List<CommunityPreviewModel> Communities { get; set; }
    public List<UserPreviewModel> Friends { get; set; }
}