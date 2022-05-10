using SocialNetwork.Models.Posts;

namespace SocialNetwork.Repository;

public interface IPostsRepository : IRepository<Post, int>
{
    public IEnumerable<Post> GetPostsFromCommunity(int communityId);
}