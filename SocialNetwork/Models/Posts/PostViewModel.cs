using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Images;

namespace SocialNetwork.Models.Posts;

public class PostViewModel
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public DateTime DateTime { get; set; }
    public bool IsUserLike { get; set; }
    public List<ImageViewModel> Images { get; set; } 
    public CommunityPreviewModel Community { get; set; }
}