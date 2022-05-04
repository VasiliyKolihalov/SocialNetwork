using SocialNetwork.Models.Users;

namespace SocialNetwork.Repository;

public interface IUsersRepository : IRepository<User, int>
{
    public User GetBasedEmail(string email);
}