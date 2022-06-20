using SocialNetwork.Models.Users;

namespace SocialNetwork.Models.Comments;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int LikesCount { get; set; }
    public int PostId { get; set; }
    public User User { get; set; }
}