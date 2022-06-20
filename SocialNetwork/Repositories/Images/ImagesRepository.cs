using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Images;

namespace SocialNetwork.Repositories.Images;

public class ImagesRepository : IImagesRepository
{
    private readonly string _connectionString;

    public ImagesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Image> GetAll()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            return connection.Query<Image>("SELECT * FROM Images");
        }
    }

    public Image Get(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            Image image = connection.QueryFirstOrDefault<Image>("SELECT * FROM Images WHERE Id = @id", new {id});

            if (image == null)
                throw new NotFoundException("Image not found");

            return image;
        }
    }

    public void Add(Image item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO Images VALUES(@ImageData, @UserId) SELECT @@IDENTITY";
            int imageId = connection.QuerySingle<int>(sqlQuery, item);
            item.Id = imageId;
        }
    }

    public void Update(Image item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE Images SET ImageData = @ImageData WHERE Id = @Id";
            connection.QuerySingle<int>(sqlQuery, item);
        }
    }

    public void Delete(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            connection.Query("DELETE Images WHERE Id = @id", new {id});
        }
    }

    public Image? GetUserAvatar(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = @"SELECT Images.Id, Images.ImageData, Images.UserId FROM Images
                                INNER JOIN UsersAvatars ON Images.Id = UsersAvatars.ImageId AND UsersAvatars.UserId = @userId";
            Image image = connection.QueryFirstOrDefault<Image>(sqlQuery, new {userId});

            return image;
        }
    }

    public IEnumerable<Image> GetUserPhotos(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = @"SELECT Images.Id, Images.ImageData, Images.UserId FROM Images
                                INNER JOIN UsersPhotos ON Images.Id = UsersPhotos.ImageId AND UsersPhotos.UserId = @userId";

            IEnumerable<Image> images = connection.Query<Image>(sqlQuery, new {userId});

            return images;
        }
    }

    public void SetDefaultValueForUserAvatar(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO UsersAvatars VALUES (@userId, NULL)";
            connection.Query(sqlQuery, new {userId});
        }
    }

    public void UpdateUserAvatar(int userId, int imageId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE UsersAvatars SET ImageId = @imageId WHERE UserId = @userId";
            connection.Query(sqlQuery, new {imageId, userId});
        }
    }
    
    public void AddPhotoToUser(int userId, int imageId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO UsersPhotos VALUES (@userId, @imageId)";
            connection.Query(sqlQuery, new {userId, imageId});
        }
    }

    public void DeletePhotoFromUser(int userId, int imageId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "DELETE UsersPhotos WHERE UserId = @userId AND ImageId = @imageId";
            connection.Query(sqlQuery, new {userId, imageId});
        }
    }

    public Image? GetCommunityAvatar(int communityId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = @"SELECT Images.Id, Images.ImageData, Images.UserId FROM Images
                                INNER JOIN CommunitiesAvatars ON Images.Id = CommunitiesAvatars.ImageId AND CommunitiesAvatars.CommunityId = @communityId";

            Image image = connection.QueryFirstOrDefault<Image>(sqlQuery, new {communityId});

            return image;
        }
    }


    public void SetDefaultValueForCommunityAvatar(int communityId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO CommunitiesAvatars VALUES (@communityId, NULL)";
            connection.Query(sqlQuery, new {communityId});
        }
    }

    public void UpdateCommunityAvatar(int communityId, int imageId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "UPDATE CommunitiesAvatars SET ImageId = @imageId WHERE CommunityId = @communityId";
            connection.Query(sqlQuery, new {communityId, imageId});
        }
    }
    
    public IEnumerable<Image> GetPostImages(int postId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = @"SELECT Images.Id, Images.ImageData, Images.UserId FROM Images
                                INNER JOIN PostsImages ON Images.Id = PostsImages.ImageId AND PostsImages.PostId = @postId";

            IEnumerable<Image> images = connection.Query<Image>(sqlQuery, new {postId});

            return images;
        }
    }

    public void AddImageToPost(int postId, int imageId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string sqlQuery = "INSERT INTO PostsImages VALUES(@postId, @imageId)";
            connection.Query(sqlQuery, new {postId, imageId});
        }
    }
}