using SocialNetwork.Models.Comments;

namespace SocialNetwork.Repository;

public interface ICommentsRepository : IRepository<Comment, int>
{
    public IEnumerable<Comment> GetPostsComment(int postId);
}