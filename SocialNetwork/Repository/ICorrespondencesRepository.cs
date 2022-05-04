using SocialNetwork.Models.Correspondences;

namespace SocialNetwork.Repository;

public interface ICorrespondencesRepository : IRepository<Correspondence, int>
{
    public IEnumerable<Correspondence> GetUserCorrespondences(int userId);
}