using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Users;


namespace SocialNetwork.Repository;

public class UsersRepository : IUsersRepository
{
    private readonly string _connectionString;

    public UsersRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<User> GetAll()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            return connection.Query<User>("SELECT * FROM Users");
        }
    }

    public User Get(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            User user = connection.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id = @id", new {id});
            
            if (user == null)
                throw new NotFoundException("User not found");
            
            return user;
        }
    }

    public User GetFromEmail(string email)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            User user = connection
                .QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Email = @email", new {email});
            
            if (user == null)
                throw new NotFoundException("User not found");
            
            return user;
        }
    }

    public void Add(User item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery =
                "INSERT INTO Users VALUES (@FirstName, @SecondName, @Email, @PasswordHash) SELECT @@IDENTITY";
            int userId = connection.QuerySingle<int>(sqlQuery, item);
            item.Id = userId;
        }
    }

    public void Update(User item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery =
                "UPDATE Users SET FirstName = @FirstName, SecondName = @SecondName, Email = @Email WHERE Id = @Id";
            connection.Query(sqlQuery, item);
        }
    }

    public void Delete(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Query("DELETE Users WHERE Id = @id", new {id});
        }
    }
}