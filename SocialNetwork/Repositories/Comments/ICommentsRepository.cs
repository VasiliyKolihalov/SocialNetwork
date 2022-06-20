using SocialNetwork.Models.Comments;
using SocialNetwork.Repositories.Likes;

namespace SocialNetwork.Repositories.Comments;

public interface ICommentsRepository : IRepository<Comment, int>, ILikedContent<int>
{
    public IEnumerable<Comment> GetPostComments(int postId);
}