using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Posts;

namespace SocialNetwork.Repository;

public class PostsRepository : IPostsRepository
{
    private readonly string _connectionString;

    public PostsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Post> GetAll()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            return connection.Query<Post>("SELECT * FROM Posts");
        }
    }
    
    public IEnumerable<Post> GetPostsFromCommunity(int communityId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            return connection.Query<Post>("SELECT * FROM Posts WHERE CommunityId = @communityId", new {communityId});
        }
    }
    
    public Post Get(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            Post post = connection.QueryFirstOrDefault<Post>("SELECT * FROM Posts WHERE Id = @id", new {id});

            if (post == null)
                throw new NotFoundException("Post not found");

            return post;
        }
    }

    public void Add(Post item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO Posts VALUES (@Content, @CommunityId) SELECT @@IDENTITY";
            int postId = connection.QuerySingle<int>(sqlQuery, item);

            item.Id = postId;
        }
    }

    public void Update(Post item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE Posts SET Content = @Content WHERE Id = @Id";
            connection.Query(sqlQuery, item);
        }
    }

    public void Delete(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Query("DELETE Posts WHERE Id = @Id", new {id});
        }
    }

   
}