using SocialNetwork.Repositories.Comments;
using SocialNetwork.Repositories.Communities;
using SocialNetwork.Repositories.Correspondences;
using SocialNetwork.Repositories.FriendRequests;
using SocialNetwork.Repositories.Images;
using SocialNetwork.Repositories.Messages;
using SocialNetwork.Repositories.Posts;
using SocialNetwork.Repositories.Roles;
using SocialNetwork.Repositories.Users;
using SocialNetwork.Repository;

namespace SocialNetwork.Repositories;

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
    public ICommentsRepository Comments { get; }

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
        Comments = new CommentsRepository(connectionString);
    }
}