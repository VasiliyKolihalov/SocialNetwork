using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Roles;
using SocialNetwork.Models.Users;
using SocialNetwork.Repositories.Roles;

namespace SocialNetwork.Repository;

public class RolesRepository : IRolesRepository
{
    private readonly string _connectionString;

    public RolesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Role> GetAll()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            return connection.Query<Role>("SELECT * FROM Roles");
        }
    }

    public Role Get(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            Role role = connection.QueryFirstOrDefault<Role>("SELECT * FROM Roles WHERE Id = @id", new {id});

            if (role == null)
                throw new NotFoundException("Role not found");

            return role;
        }
    }

    public void Add(Role item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            int roleId = connection.QuerySingle<int>("INSERT INTO Roles VALUES (@Name) SELECT @@IDENTITY", item);
            item.Id = roleId;
        }
    }

    public void Update(Role item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Query("UPDATE Roles SET Name = @Name WHERE Id = @Id", item);
        }
    }

    public void Delete(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Query("DELETE Roles WHERE Id = @id", new {id});
        }
    }
    
    public IEnumerable<Role> GetFromUserId(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string rolesQuery =
                @"SELECT Roles.Id, Roles.Name FROM Roles INNER JOIN UsersRoles ON Roles.Id = UsersRoles.RoleId AND UsersRoles.UserId = @userId";
            IEnumerable<Role> roles = connection.Query<Role>(rolesQuery, new {userId});
            return roles;
        }
    }

    public Role GetFromName(string name)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            Role role = connection.QueryFirstOrDefault<Role>("SELECT * FROM Roles WHERE Name = @name", new {name});

            if (role == null)
                throw new NotFoundException("Role not found");

            return role;
        }
    }
    
    public void AddRoleToUser(int userId, int roleId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string userRoleQuery = "INSERT INTO UsersRoles VALUES (@userId, @roleId)";
            connection.Query(userRoleQuery, new {userId, roleId});
        }
    }
}