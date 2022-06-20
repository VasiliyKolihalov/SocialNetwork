namespace SocialNetwork.Repositories.Likes;

public interface ILikedContent<TId>
{
    public bool IsUserLike(int userId, TId contentId);
    public void AddLike(int userId, TId contentId);
    public void DeleteLike(int userId, TId contentId);
}