using SocialNetwork.Models.Correspondences;

namespace SocialNetwork.Repositories.Correspondences;

public interface ICorrespondencesRepository : IRepository<Correspondence, int>
{
    public IEnumerable<Correspondence> GetUserCorrespondences(int userId);

    public void AddUserToCorrespondence(int userId, int correspondenceId);

    public void DeleteUserFromCorrespondence(int userId, int correspondenceId);
}