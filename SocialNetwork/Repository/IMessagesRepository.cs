using SocialNetwork.Models.Messages;

namespace SocialNetwork.Repository;

public interface IMessagesRepository : IRepository<Message, long>
{
    public IEnumerable<Message> GetFromCorrespondence(int correspondenceId);
}