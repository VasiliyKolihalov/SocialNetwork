using SocialNetwork.Models.Posts;
using SocialNetwork.Repositories.Likes;

namespace SocialNetwork.Repositories.Posts;

public interface IPostsRepository : IRepository<Post, int>, ILikedContent<int>
{
    public IEnumerable<Post> GetPostsFromCommunity(int communityId);
}