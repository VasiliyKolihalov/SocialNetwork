using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.Comments;

public class CommentViewModel
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int LikesCount { get; set; }
    public bool IsUserLike { get; set; }
    public UserPreviewModel User { get; set; }
}