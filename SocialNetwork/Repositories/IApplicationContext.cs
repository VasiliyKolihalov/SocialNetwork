using SocialNetwork.Repositories.Comments;
using SocialNetwork.Repositories.Communities;
using SocialNetwork.Repositories.Correspondences;
using SocialNetwork.Repositories.FriendRequests;
using SocialNetwork.Repositories.Images;
using SocialNetwork.Repositories.Messages;
using SocialNetwork.Repositories.Posts;
using SocialNetwork.Repositories.Roles;
using SocialNetwork.Repositories.Users;

namespace SocialNetwork.Repositories;

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