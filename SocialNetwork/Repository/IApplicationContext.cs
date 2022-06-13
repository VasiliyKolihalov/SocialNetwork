namespace SocialNetwork.Repository;

public interface IApplicationContext
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

}