
namespace SocialNetwork.Repository;

public class ApplicationContext
{
    public UsersRepository Users { get; }
    public RolesRepository Roles { get; }
    public CorrespondencesRepository Correspondences { get; }
    public MessagesRepository Messages { get; }
     
    public ApplicationContext(string connectionString)
    {
        Users = new UsersRepository(connectionString);
        Roles = new RolesRepository(connectionString);
        Correspondences = new CorrespondencesRepository(connectionString);
        Messages = new MessagesRepository(connectionString);
    }
}