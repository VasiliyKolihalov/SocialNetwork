using SocialNetwork.Models.Communities;

namespace SocialNetwork.Models.Posts;

public class PostViewModel
{
    public int Id { get; set; }
    public string Content { get; set; }
    public CommunityPreviewModel Community { get; set; }
}