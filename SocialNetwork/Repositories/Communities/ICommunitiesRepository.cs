using SocialNetwork.Models.Communities;

namespace SocialNetwork.Repositories.Communities;

public interface ICommunitiesRepository : IRepository<Community, int>
{
    public IEnumerable<Community> GetFollowedCommunity(int userId);
    public IEnumerable<Community> GetManagedCommunity(int userId);
    public void SubscribeUserToCommunity(int communityId, int userId);
    public void UnsubscribeUserFromCommunity(int communityId, int userId);

}