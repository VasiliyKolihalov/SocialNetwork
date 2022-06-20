using SocialNetwork.Models.Messages;

namespace SocialNetwork.Repositories.Messages;

public interface IMessagesRepository : IRepository<Message, long>
{
    public IEnumerable<Message> GetFromCorrespondence(int correspondenceId);
}