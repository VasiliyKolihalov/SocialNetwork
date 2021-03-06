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

    public void FreezeUser(int userId);
    public void UnfreezeUser(int userId);

    public void ChangePasswordHash(int userId, string passwordHash);

    public string? GetEmailConfirmCode(int userId);
    public void AddEmailConfirmCode(int userId, string code);
    public void DeleteEmailConfirmCode(int userId);
    public void ConfirmUserEmail(int userId);

}