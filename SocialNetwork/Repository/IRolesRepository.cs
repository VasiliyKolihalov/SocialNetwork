using SocialNetwork.Models.Roles;

namespace SocialNetwork.Repository;

public interface IRolesRepository : IRepository<Role, int>
{
    public Role GetBasedName(string name);
    public IEnumerable<Role> GetBasedUserId(int userId);
    public void AddRoleToUser(int userId, int roleId);
}