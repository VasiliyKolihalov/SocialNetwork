using SocialNetwork.Models.Communities;

namespace SocialNetwork.Models.Posts;

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int CommunityId { get; set; }
}