using SocialNetwork.Models.Posts;

namespace SocialNetwork.Repository;

public interface IPostsRepository : IRepository<Post, int>
{
    public IEnumerable<Post> GetPostsFromCommunity(int communityId);
    
    public bool IsUserLikePost(int userId, int postId);
    public void AddLikeToPost(int userId, int postId);
    public void DeleteLikeFromPost(int userId, int postId);
}