using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Repository;

public interface IUsersRepository : IRepository<User, int>
{
    public User GetFromEmail(string email);
    public IEnumerable<User> GetUserFriends(int userId);
    public void AddUserToFriends(int userId, int friendId);
    public void DeleteUserFromFriend(int userId, int friendId);
   
}