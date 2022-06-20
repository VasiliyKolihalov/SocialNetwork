using SocialNetwork.Models.Users;

namespace SocialNetwork.Repositories.Users;

public interface IUsersRepository : IRepository<User, int>
{
    public User GetFromEmail(string email);
    public IEnumerable<User> GetUserFriends(int userId);
    public void AddUserToFriends(int userId, int friendId);
    public void DeleteUserFromFriend(int userId, int friendId);

    public IEnumerable<User> GetUsersWhoLikePost(int postId);
    public IEnumerable<User> GetUsersWhoLikeComment(int commentId);
}