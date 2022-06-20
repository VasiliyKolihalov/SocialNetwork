using SocialNetwork.Models.FriendsRequests;

namespace SocialNetwork.Repositories.FriendRequests;

public interface IFriendRequestsRepository : IRepository<FriendRequest, int>
{
    public IEnumerable<FriendRequest> GetUserFriendsRequests(int userId);
}