using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Repositories.Users;

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

    public void Add(User item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery =
                "INSERT INTO Users VALUES (@FirstName, @SecondName, @Email, @PasswordHash, DEFAULT, DEFAULT) SELECT @@IDENTITY";
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

    public IEnumerable<User> GetUserFriends(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = @"SELECT * FROM Users WHERE Id IN 
            ((SELECT SecondUser AS Id FROM UsersFriends WHERE FirstUser = @userId) UNION (SELECT FirstUser AS Id FROM UsersFriends WHERE SecondUser = @userId))";

            IEnumerable<User> users = connection.Query<User>(sqlQuery, new {userId});
            return users;
        }
    }

    public void AddUserToFriends(int userId, int friendId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO UsersFriends VALUES(@userId, @friendId)";
            connection.Query(sqlQuery, new {userId, friendId});
        }
    }

    public void DeleteUserFromFriend(int userId, int friendId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery =
                "DELETE UsersFriends WHERE FirstUser = @userId OR SecondUser = @userId AND FirstUser = @friendId OR SecondUser = @friendId";
            connection.Query(sqlQuery, new {userId, friendId});
        }
    }

    public IEnumerable<User> GetUsersWhoLikePost(int postId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash, Users.IsFreeze FROM Users 
                                 INNER JOIN PostsLikes ON Users.Id = PostsLikes.UserId AND PostsLikes.PostId = postId";

            IEnumerable<User> users = connection.Query<User>(sqlQuery, new {postId});
            return users;
        }
    }

    public IEnumerable<User> GetUsersWhoLikeComment(int commentId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash, Users.IsFreeze FROM Users 
                                 INNER JOIN CommentsLikes ON Users.Id = CommentsLikes.UserId AND CommentsLikes.CommentId = commentId";

            IEnumerable<User> users = connection.Query<User>(sqlQuery, new {commentId});
            return users;
        }
    }

    public void FreezeUser(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE Users SET IsFreeze = @Value WHERE Id = @Id";
            connection.Query(sqlQuery, new {Value = true, Id = userId});
        }
    }

    public void UnfreezeUser(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE Users SET IsFreeze = @Value WHERE Id = @Id";
            connection.Query(sqlQuery, new {Value = false, Id = userId});
        }
    }

    public void ChangePasswordHash(int userId, string passwordHash)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE Users SET PasswordHash = @passwordHash WHERE Id = @userId";
            connection.Query(sqlQuery, new {passwordHash, userId});
        }
    }

    public string? GetEmailConfirmCode(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "SELECT Code FROM UsersEmailConfirmCodes WHERE UserId = @userId";
            string? code = connection.QueryFirstOrDefault<string>(sqlQuery, new {userId});
            return code;
        }
    }

    public void AddEmailConfirmCode(int userId, string code)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO UsersEmailConfirmCodes VALUES (@userId, @code)";
            connection.Query(sqlQuery, new {userId, code});
        }
    }

    public void DeleteEmailConfirmCode(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "DELETE UsersEmailConfirmCodes WHERE UserId = @userId";
            connection.Query(sqlQuery, new {userId});
        }
    }

    public void ConfirmUserEmail(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE Users SET IsConfirmEmail = @Value WHERE Id = @Id";
            connection.Query(sqlQuery, new {Value = true, Id = userId});
        }
    }
}