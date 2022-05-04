using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Correspondences;
using SocialNetwork.Models.Messages;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Repository;

public class MessagesRepository : IMessagesRepository
{
    private readonly string _connectionString;

    public MessagesRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Message> GetAll()
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            IEnumerable<Message> messages = connection.Query<Message>("SELECT * FROM Messages ");

            string userMessageQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users 
                INNER JOIN Messages ON Users.Id = Messages.UserId AND Messages.Id = @Id";

            string correspondenceQuery = @"SELECT Correspondences.Id, Correspondences.Name FROM Correspondences
                INNER JOIN Messages ON Correspondences.Id = Messages.CorrespondencesId AND Messages.Id = @Id";

            string usersCorrespondenceQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                INNER JOIN UsersCorrespondences ON Users.Id = UsersCorrespondences.UserId AND UsersCorrespondences.CorrespondenceId = @Id";

            foreach (var message in messages)
            {
                message.Sender = connection.QuerySingle<User>(userMessageQuery, new {Id = message.Id});

                message.Correspondence =
                    connection.QuerySingle<Correspondence>(correspondenceQuery, new {Id = message.Id});
                message.Correspondence.Users =
                    connection.Query<User>(usersCorrespondenceQuery, new {Id = message.Correspondence.Id}).ToList();
            }

            return messages;
        }
    }

    public IEnumerable<Message> GetBasedCorrespondence(int correspondenceId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            IEnumerable<Message> messages =
                connection.Query<Message>("SELECT * FROM Messages WHERE CorrespondenceId = @Id", new {Id = correspondenceId});

            string userMessageQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users 
                INNER JOIN Messages ON Users.Id = Messages.UserId AND Messages.Id = @Id";
            
            string correspondenceQuery = "SELECT * FROM Correspondences WHERE Id = @Id";

            string usersCorrespondenceQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                INNER JOIN UsersCorrespondences ON Users.Id = UsersCorrespondences.UserId AND UsersCorrespondences.CorrespondenceId = @Id";

            foreach (var message in messages)
            {
                message.Sender = connection.QuerySingle<User>(userMessageQuery, new {Id = message.Id});

                message.Correspondence =
                    connection.QuerySingle<Correspondence>(correspondenceQuery, new {Id = correspondenceId});
                message.Correspondence.Users =
                    connection.Query<User>(usersCorrespondenceQuery, new {Id = correspondenceId}).ToList();
            }

            return messages;
        }
    }

    public Message Get(long id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            Message message =
                connection.QueryFirstOrDefault<Message>("SELECT * FROM Messages WHERE Id = @id", new {id});

            if (message == null)
                throw new NotFoundException("Message not found");

            string userMessageQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users 
                INNER JOIN Messages ON Users.Id = Messages.UserId AND Messages.Id = @Id";

            string correspondenceQuery = @"SELECT Correspondences.Id, Correspondences.Name FROM Correspondences
                INNER JOIN Messages ON Correspondences.Id = Messages.CorrespondenceId AND Messages.Id = @Id";

            string usersCorrespondenceQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash FROM Users
                INNER JOIN UsersCorrespondences ON Users.Id = UsersCorrespondences.UserId AND UsersCorrespondences.CorrespondenceId = @Id";

            message.Sender = connection.QuerySingle<User>(userMessageQuery, new {Id = message.Id});
            message.Correspondence = connection.QuerySingle<Correspondence>(correspondenceQuery, new {Id = message.Id});
            message.Correspondence.Users = connection.Query<User>(usersCorrespondenceQuery, new {Id = message.Correspondence.Id}).ToList();

            return message;
        }
    }


    public void Add(Message item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery =
                "INSERT INTO Messages VALUES (@Text, @DateTime, @IsEdited, @UserId, @CorrespondenceId) SELECT @@IDENTITY";
            int messagesId = connection.QuerySingle<int>(sqlQuery,
                new
                {
                    Text = item.Text, DateTime = DateTime.Now, IsEdited = false,
                    UserId = item.Sender.Id, CorrespondenceId = item.Correspondence.Id
                });
            item.Id = messagesId;
        }
    }

    public void Update(Message item)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery =
                "UPDATE Messages SET Text = @Text, IsEdited = @IsEdited WHERE Id = @Id";
            connection.Query(sqlQuery, new {Text = item.Text, IsEdited = true, Id = item.Id});
        }
    }

    public void Delete(long id)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            var sqlQuery = "DELETE Messages WHERE Id = @id";
            connection.Query(sqlQuery, new {id});
        }
    }
}