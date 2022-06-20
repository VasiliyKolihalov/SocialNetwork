using SocialNetwork.Models.Roles;

namespace SocialNetwork.Repositories.Roles;

public interface IRolesRepository : IRepository<Role, int>
{
    public Role GetFromName(string name);
    public IEnumerable<Role> GetFromUserId(int userId);
    public void AddRoleToUser(int userId, int roleId);
}