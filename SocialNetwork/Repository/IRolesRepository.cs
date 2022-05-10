using SocialNetwork.Models.Roles;

namespace SocialNetwork.Repository;

public interface IRolesRepository : IRepository<Role, int>
{
    public Role GetFromName(string name);
    public IEnumerable<Role> GetFromUserId(int userId);
    public void AddRoleToUser(int userId, int roleId);
}