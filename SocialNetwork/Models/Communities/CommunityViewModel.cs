using SocialNetwork.Models.Posts;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.Communities;

public class CommunityViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<PostViewModel> Posts { get; set; }
    public UserViewModel Author { get; set; }
    public List<UserViewModel> Users { get; set; }
}