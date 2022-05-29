using SocialNetwork.Models.FriendsRequests;

namespace SocialNetwork.Repository;

public interface IFriendRequestsRepository : IRepository<FriendRequest, int>
{
    public IEnumerable<FriendRequest> GetUserFriendsRequests(int userId);
}