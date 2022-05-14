using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Correspondences;
using SocialNetwork.Models.Messages;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Repository;

public class CorrespondencesRepository : ICorrespondencesRepository
{
    private readonly string _connectionString;

    public CorrespondencesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Correspondence> GetAll()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            IEnumerable<Correspondence> correspondences = connection.Query<Correspondence>("SELECT * FROM Correspondences");
            
            string usersQuery = @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                INNER JOIN UsersCorrespondences ON Users.Id = UsersCorrespondences.UserId AND UsersCorrespondences.CorrespondenceId = @Id";

            string adminQuery = @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                        INNER JOIN Correspondences ON Users.Id = Correspondences.AdminId AND Correspondences.Id = @Id";
            foreach (var correspondence in correspondences)
            {
                correspondence.Users = connection.Query<User>(usersQuery, new {Id = correspondence.Id}).ToList();
                correspondence.Admin = connection.QuerySingle<User>(adminQuery, new {Id = correspondence.Id});
            }

            return correspondences;
        }
    }

    public Correspondence Get(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            Correspondence correspondence = connection
                .QueryFirstOrDefault<Correspondence>("SELECT * FROM Correspondences WHERE Id = @id", new {id});
            
            if (correspondence == null)
                throw new NotFoundException("Correspondence not found");
            
            string usersQuery = @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                INNER JOIN UsersCorrespondences ON Users.Id = UsersCorrespondences.UserId AND UsersCorrespondences.CorrespondenceId = @Id";
            
            string adminQuery = @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                        INNER JOIN Correspondences ON Users.Id = Correspondences.AdminId AND Correspondences.Id = @Id";
            
            correspondence.Users = connection.Query<User>(usersQuery, new {Id = correspondence.Id}).ToList();
            correspondence.Admin = connection.QuerySingle<User>(adminQuery, new {Id = correspondence.Id});

            return correspondence;
        }
    }

    public IEnumerable<Correspondence> GetUserCorrespondences(int userId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            string correspondencesQuery = @"SELECT Correspondences.Id, Correspondences.Name FROM Correspondences 
                INNER JOIN UsersCorrespondences ON Correspondences.Id = UsersCorrespondences.CorrespondenceId AND UsersCorrespondences.UserId = @userId";
            
            IEnumerable<Correspondence> correspondences = connection.Query<Correspondence>(correspondencesQuery, new {userId});

            string usersQuery = @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                INNER JOIN UsersCorrespondences ON Users.Id = UsersCorrespondences.UserId AND UsersCorrespondences.CorrespondenceId = @Id";
            
            string adminQuery = @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                                        INNER JOIN Correspondences ON Users.Id = Correspondences.AdminId AND Correspondences.Id = @Id";
            
            foreach (var correspondence in correspondences)
            {
                correspondence.Users = connection.Query<User>(usersQuery, new {Id = correspondence.Id}).ToList();
                correspondence.Admin = connection.QuerySingle<User>(adminQuery, new {Id = correspondence.Id});
            }

            return correspondences;
        }
    }

    public void AddUserToCorrespondence(int userId, int correspondenceId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var usersQuery = "INSERT INTO UsersCorrespondences VALUES (@userId, @correspondenceId)";
            connection.Query(usersQuery, new {userId, correspondenceId});
        }
    }

    public void DeleteUserFromCorrespondence(int userId, int correspondenceId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var usersQuery = "DELETE UsersCorrespondences WHERE UserId = @userId AND CorrespondenceId = @correspondenceId";
            connection.Query(usersQuery, new {userId, correspondenceId});
        }
    }

    public void Add(Correspondence item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var correspondenceQuery =
                "INSERT INTO Correspondences VALUES (@Name, @AdminId) SELECT @@IDENTITY";
            int correspondenceId = connection.QuerySingle<int>(correspondenceQuery, new{Name = item.Name, AdminId = item.Admin.Id});
            item.Id = correspondenceId;

            var usersQuery = "INSERT INTO UsersCorrespondences VALUES (@UserId, @CorrespondenceId)";
            foreach (var user in item.Users)
            {
                connection.Query(usersQuery, new {UserId = user.Id, CorrespondenceId = item.Id});
            }
        }
    }
    
    public void Update(Correspondence item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery =
                "UPDATE Correspondences SET Name = @Name WHERE Id = @Id";
            connection.Query(sqlQuery, item);
        }
    }

    public void Delete(int id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery = "DELETE Correspondences WHERE Id = @id";
            connection.Query(sqlQuery, new {id});
        }
    }
}