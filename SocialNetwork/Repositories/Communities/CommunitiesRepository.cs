using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Repositories.Communities;

public class CommunitiesRepository : ICommunitiesRepository
{
    private readonly string _connectionString;

    public CommunitiesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Community> GetAll()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            IEnumerable<Community> communities = connection.Query<Community>("SELECT * FROM Communities");

            string authorQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN Communities ON Users.Id = Communities.AuthorId AND Communities.Id = @Id";

            string usersQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN UsersCommunities ON Users.Id = UsersCommunities.UserId AND UsersCommunities.CommunityId = @Id";

            foreach (var community in communities)
            {
                community.Author = connection.QuerySingle<User>(authorQuery, new {Id = community.Id});
                community.Users = connection.Query<User>(usersQuery, new {Id = community.Id}).ToList();
            }

            return communities;
        }
    }

    public Community Get(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            Community community =
                connection.QueryFirstOrDefault<Community>("SELECT * FROM Communities WHERE Id = @id", new {id});
            if (community == null)
                throw new NotFoundException("Community not found");

            string authorQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN Communities ON Users.Id = Communities.AuthorId AND Communities.Id = @Id";

            string usersQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN UsersCommunities ON Users.Id = UsersCommunities.UserId AND UsersCommunities.CommunityId = @Id";

            community.Author = connection.QuerySingle<User>(authorQuery, new {Id = community.Id});
            community.Users = connection.Query<User>(usersQuery, new {Id = community.Id}).ToList();

            return community;
        }
    }


    public void Add(Community item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery =
                "INSERT INTO Communities VALUES (@Name, @Description, @AuthorId) SELECT @@IDENTITY";

            int communityId = connection.QuerySingle<int>(sqlQuery,
                new
                {
                    Name = item.Name, Description = item.Description,
                    AuthorId = item.Author.Id
                });
            item.Id = communityId;
        }
    }

    public void Update(Community item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery =
                "UPDATE Communities SET Name = @Name, Description = @Description WHERE Id = @Id";
            connection.Query(sqlQuery, item);
        }
    }

    public void Delete(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Query("DELETE Communities WHERE Id = @id", new {id});
        }
    }

    public IEnumerable<Community> GetFollowedCommunity(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string communitiesQuery =
                @"SELECT Communities.Id, Communities.Name, Communities.Description FROM Communities
                                          INNER JOIN UsersCommunities ON Communities.Id = UsersCommunities.CommunityId AND UsersCommunities.UserId = @userId";
            IEnumerable<Community> communities = connection.Query<Community>(communitiesQuery, new {userId});

            string authorQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN Communities ON Users.Id = Communities.AuthorId AND Communities.Id = @Id";

            string usersQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN UsersCommunities ON Users.Id = UsersCommunities.UserId AND UsersCommunities.CommunityId = @Id";

            foreach (var community in communities)
            {
                community.Author = connection.QuerySingle<User>(authorQuery, new {Id = community.Id});
                community.Users = connection.Query<User>(usersQuery, new {Id = community.Id}).ToList();
            }

            return communities;
        }
    }

    public IEnumerable<Community> GetManagedCommunity(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            IEnumerable<Community> communities =
                connection.Query<Community>("SELECT * FROM Communities WHERE AuthorId = @userId", new {userId});

            string authorQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN Communities ON Users.Id = Communities.AuthorId AND Communities.Id = @Id";

            string usersQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                    INNER JOIN UsersCommunities ON Users.Id = UsersCommunities.UserId AND UsersCommunities.CommunityId = @Id";

            foreach (var community in communities)
            {
                community.Author = connection.QuerySingle<User>(authorQuery, new {Id = community.Id});
                community.Users = connection.Query<User>(usersQuery, new {Id = community.Id}).ToList();
            }

            return communities;
        }
    }

    public void SubscribeUserToCommunity(int communityId, int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string userCommunityQuery = "INSERT INTO UsersCommunities VALUES (@userId, @communityId)";
            connection.Query(userCommunityQuery, new {userId, communityId});
        }
    }

    public void UnsubscribeUserFromCommunity(int communityId, int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string userCommunityQuery = "DELETE UsersCommunities WHERE UserId = @userId AND CommunityId = @communityId";
            connection.Query(userCommunityQuery, new {userId, communityId});
        }
    }
}