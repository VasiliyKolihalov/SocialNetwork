namespace SocialNetwork.Repository;

public class ApplicationContext : IApplicationContext
{
    public IUsersRepository Users { get; }
    public IRolesRepository Roles { get; }
    public ICorrespondencesRepository Correspondences { get; }
    public IMessagesRepository Messages { get; }
    public ICommunitiesRepository Communities { get; }
    public IPostsRepository Posts { get; }
    public IFriendRequestsRepository FriendRequests { get; }
    public IImagesRepository Images { get; }

    public ApplicationContext(string connectionString)
    {
        Users = new UsersRepository(connectionString);
        Roles = new RolesRepository(connectionString);
        Correspondences = new CorrespondencesRepository(connectionString);
        Messages = new MessagesRepository(connectionString);
        Communities = new CommunitiesRepository(connectionString);
        Posts = new PostsRepository(connectionString);
        FriendRequests = new FriendRequestsRepository(connectionString);
        Images = new ImagesRepository(connectionString);
    }
}