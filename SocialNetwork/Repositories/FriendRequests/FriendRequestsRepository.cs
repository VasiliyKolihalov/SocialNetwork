using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.FriendsRequests;
using SocialNetwork.Models.Users;
using SocialNetwork.Repository;

namespace SocialNetwork.Repositories.FriendRequests;

public class FriendRequestsRepository : IFriendRequestsRepository
{
    private readonly string _connectionString;

    public FriendRequestsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<FriendRequest> GetAll()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            IEnumerable<FriendRequest> friendRequests = connection.Query<FriendRequest>("SELECT * FROM FriendRequests");
            string senderQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN FriendRequests ON Users.Id = FriendRequests.SenderId AND FriendRequests.Id = @Id";
            foreach (var friendRequest in friendRequests)
            {
                friendRequest.Sender = connection.QuerySingle<User>(senderQuery, new {Id = friendRequest.Id});
            }

            return friendRequests;
        }
    }

    public FriendRequest Get(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            FriendRequest friendRequest =
                connection.QueryFirstOrDefault<FriendRequest>("SELECT * FROM FriendRequests WHERE Id = @id", new {id});

            if (friendRequest == null)
                throw new NotFoundException("Friend request not found");

            string senderQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN FriendRequests ON Users.Id = FriendRequests.SenderId AND FriendRequests.Id = @Id";
            friendRequest.Sender = connection.QuerySingle<User>(senderQuery, new {Id = friendRequest.Id});

            return friendRequest;
        }
    }

    public void Add(FriendRequest item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO FriendRequests VALUES(@Message, @SenderId, @RecipientId) SELECT @@IDENTITY";

            int friendRequestId = connection.QuerySingle<int>(sqlQuery,
                new
                {
                    Message = item.Message,
                    SenderId = item.Sender.Id,
                    RecipientId = item.RecipientId
                });
            item.Id = friendRequestId;
        }
    }

    public void Update(FriendRequest item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE FriendRequests SET Message = @Message WHERE Id = @Id";
            connection.Query(sqlQuery, item);
        }
    }

    public void Delete(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Query("DELETE FriendRequests WHERE Id = @id", new {id});
        }
    }

    public IEnumerable<FriendRequest> GetUserFriendsRequests(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string friendRequestsQuery = "SELECT * FROM FriendRequests WHERE RecipientId = @userId";
            IEnumerable<FriendRequest> friendRequests =
                connection.Query<FriendRequest>(friendRequestsQuery, new {userId});

            string senderQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN FriendRequests ON Users.Id = FriendRequests.SenderId AND FriendRequests.Id = @Id";
            foreach (var friendRequest in friendRequests)
            {
                friendRequest.Sender = connection.QuerySingle<User>(senderQuery, new {Id = friendRequest.Id});
            }

            return friendRequests;
        }
    }
}