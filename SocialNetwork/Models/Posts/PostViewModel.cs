using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Images;

namespace SocialNetwork.Models.Posts;

public class PostViewModel
{
    public int Id { get; set; }
    public string Content { get; set; }
    public List<ImageViewModel> Images { get; set; } 
    public CommunityPreviewModel Community { get; set; }
}