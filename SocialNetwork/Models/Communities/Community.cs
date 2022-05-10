using SocialNetwork.Models.Posts;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.Communities;

public class Community
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Post> Posts { get; set; }
    public User Author { get; set; }
    public List<User> Users { get; set; }
}