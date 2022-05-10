using SocialNetwork.Models.Communities;
using SocialNetwork.Models.Users;

namespace SocialNetwork.Repository;

public interface ICommunitiesRepository : IRepository<Community, int>
{
    public IEnumerable<Community> GetFollowedCommunity(int userId);
    public IEnumerable<Community> GetManagedCommunity(int userId);
    public void SubscribeUserToCommunity(int communityId, int userId);
    public void UnsubscribeUserFromCommunity(int communityId, int userId);

}