using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SocialNetwork.Exceptions;
using SocialNetwork.Models.Messages;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Repositories.Messages;

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

            string userQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash, Users.IsFreeze FROM Users 
                INNER JOIN Messages ON Users.Id = Messages.UserId AND Messages.Id = @Id";

            foreach (var message in messages)
            {
                message.Sender = connection.QuerySingle<User>(userQuery, new {Id = message.Id});
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

            string userQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash, Users.IsFreeze FROM Users 
                INNER JOIN Messages ON Users.Id = Messages.UserId AND Messages.Id = @Id";

            message.Sender = connection.QuerySingle<User>(userQuery, new {Id = message.Id});

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
                    UserId = item.Sender.Id, CorrespondenceId = item.CorrespondenceId
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

            item.IsEdited = true;
            connection.Query(sqlQuery, item);
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

    public IEnumerable<Message> GetFromCorrespondence(int correspondenceId)
    {
        using (IDbConnection connection = new SqlConnection(_connectionString))
        {
            IEnumerable<Message> messages =
                connection.Query<Message>("SELECT * FROM Messages WHERE CorrespondenceId = @Id",
                    new {Id = correspondenceId});

            string userQuery =
                @"SELECT Users.Id, Users.FirstName, Users.SecondName, Users.Email, Users.PasswordHash, Users.IsFreeze FROM Users 
                INNER JOIN Messages ON Users.Id = Messages.UserId AND Messages.Id = @Id";

            foreach (var message in messages)
            {
                message.Sender = connection.QuerySingle<User>(userQuery, new {Id = message.Id});
            }

            return messages;
        }
    }
}